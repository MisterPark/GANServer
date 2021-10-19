#from typing import Protocol
from PIL.TiffImagePlugin import TiffImageFile
import torch
import os
import numpy as np
import argparse
from PIL import Image, TiffImagePlugin
from torch.serialization import MAGIC_NUMBER
import torchvision.transforms as transforms
from torch.autograd import Variable
import torchvision.utils as vutils
from network.Transformer import Transformer
import socket
from threading import Thread
from queue import Queue
import io
import torch
import math
irange = range

# GAN

def make_grid(tensor, nrow=8, padding=2,
              normalize=False, range=None, scale_each=False, pad_value=0):
    """Make a grid of images.

    Args:
        tensor (Tensor or list): 4D mini-batch Tensor of shape (B x C x H x W)
            or a list of images all of the same size.
        nrow (int, optional): Number of images displayed in each row of the grid.
            The final grid size is ``(B / nrow, nrow)``. Default: ``8``.
        padding (int, optional): amount of padding. Default: ``2``.
        normalize (bool, optional): If True, shift the image to the range (0, 1),
            by the min and max values specified by :attr:`range`. Default: ``False``.
        range (tuple, optional): tuple (min, max) where min and max are numbers,
            then these numbers are used to normalize the image. By default, min and max
            are computed from the tensor.
        scale_each (bool, optional): If ``True``, scale each image in the batch of
            images separately rather than the (min, max) over all images. Default: ``False``.
        pad_value (float, optional): Value for the padded pixels. Default: ``0``.

    Example:
        See this notebook `here <https://gist.github.com/anonymous/bf16430f7750c023141c562f3e9f2a91>`_

    """
    if not (torch.is_tensor(tensor) or
            (isinstance(tensor, list) and all(torch.is_tensor(t) for t in tensor))):
        raise TypeError('tensor or list of tensors expected, got {}'.format(type(tensor)))

    # if list of tensors, convert to a 4D mini-batch Tensor
    if isinstance(tensor, list):
        tensor = torch.stack(tensor, dim=0)

    if tensor.dim() == 2:  # single image H x W
        tensor = tensor.unsqueeze(0)
    if tensor.dim() == 3:  # single image
        if tensor.size(0) == 1:  # if single-channel, convert to 3-channel
            tensor = torch.cat((tensor, tensor, tensor), 0)
        tensor = tensor.unsqueeze(0)

    if tensor.dim() == 4 and tensor.size(1) == 1:  # single-channel images
        tensor = torch.cat((tensor, tensor, tensor), 1)

    if normalize is True:
        tensor = tensor.clone()  # avoid modifying tensor in-place
        if range is not None:
            assert isinstance(range, tuple), \
                "range has to be a tuple (min, max) if specified. min and max are numbers"

        def norm_ip(img, min, max):
            img.clamp_(min=min, max=max)
            img.add_(-min).div_(max - min + 1e-5)

        def norm_range(t, range):
            if range is not None:
                norm_ip(t, range[0], range[1])
            else:
                norm_ip(t, float(t.min()), float(t.max()))

        if scale_each is True:
            for t in tensor:  # loop over mini-batch dimension
                norm_range(t, range)
        else:
            norm_range(tensor, range)

    if tensor.size(0) == 1:
        return tensor.squeeze(0)

    # make the mini-batch of images into a grid
    nmaps = tensor.size(0)
    xmaps = min(nrow, nmaps)
    ymaps = int(math.ceil(float(nmaps) / xmaps))
    height, width = int(tensor.size(2) + padding), int(tensor.size(3) + padding)
    num_channels = tensor.size(1)
    grid = tensor.new_full((num_channels, height * ymaps + padding, width * xmaps + padding), pad_value)
    k = 0
    for y in irange(ymaps):
        for x in irange(xmaps):
            if k >= nmaps:
                break
            grid.narrow(1, y * height + padding, height - padding)\
                .narrow(2, x * width + padding, width - padding)\
                .copy_(tensor[k])
            k = k + 1
    return grid

def save_image(tensor, nrow=8, padding=2,
               normalize=False, range=None, scale_each=False, pad_value=0, format=None):
    """Save a given Tensor into an image file.

    Args:
        tensor (Tensor or list): Image to be saved. If given a mini-batch tensor,
            saves the tensor as a grid of images by calling ``make_grid``.
        fp - A filename(string) or file object
        format(Optional):  If omitted, the format to use is determined from the filename extension.
            If a file object was used instead of a filename, this parameter should always be used.
        **kwargs: Other arguments are documented in ``make_grid``.
    """
    from PIL import Image
    grid = make_grid(tensor, nrow=nrow, padding=padding, pad_value=pad_value,
                     normalize=normalize, range=range, scale_each=scale_each)
    # Add 0.5 after unnormalizing to [0, 255] to round to nearest integer
    ndarr = grid.mul(255).add_(0.5).clamp_(0, 255).permute(1, 2, 0).to('cpu', torch.uint8).numpy()
    im = Image.fromarray(ndarr)
    return im


def cartoonGAN(input:Image):
	model = Transformer()
	model.load_state_dict(torch.load(os.path.join('./pretrained_model', 'Hayao' + '_net_G_float.pth')))
	model.eval()
	model.float()

	input_image = input.convert("RGB")
	# resize image, keep aspect ratio
	h = input_image.size[0]
	w = input_image.size[1]
	ratio = h *1.0 / w
	if ratio > 1:
		h = 450
		w = int(h*1.0/ratio)
	else:
		w = 450
		h = int(w * ratio)
	input_image = input_image.resize((h, w), Image.BICUBIC)
	input_image = np.asarray(input_image)
	# RGB -> BGR
	input_image = input_image[:, :, [2, 1, 0]]
	input_image = transforms.ToTensor()(input_image).unsqueeze(0)
	# preprocess, (-1, 1)
	input_image = -1 + 2 * input_image 
	input_image = Variable(input_image, volatile=True).float()
		
	# forward
	output_image = model(input_image)
	output_image = output_image[0]
	# BGR -> RGB
	output_image = output_image[[2, 1, 0], :, :]
	# deprocess, (0, 1)
	output_image = output_image.data.cpu().float() * 0.5 + 0.5

	output = save_image(output_image)

	return output


# const
HOST = '127.0.0.1'
PORT = 9999
PACKET_CODE = int('0x77',16)
HEADER_SIZE = 8

# global
jobQ = Queue()
listenSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
threadList = []
isShutdown = False

# function
def read_buffer(buffer:bytearray, count:int):
	if(len(buffer) < count):
		return None
	ret = bytearray()
	for i in range(count):
		ret.append(buffer[0])
		buffer.pop(0)
	return ret

def peek_buffer(buffer:bytearray, count:int):
	if(len(buffer)< count):
		return None
	ret = bytearray()
	for i in range(count):
		ret.append(buffer[i])
	return ret


def image_to_byte_array(image:Image):
	imgByteArr = io.BytesIO()
	image.save(imgByteArr, 'JPEG')
	imgByteArr = imgByteArr.getvalue()
	return imgByteArr

def byte_array_to_image(binary:bytes):
	imgStream = io.BytesIO(binary)
	img = Image.open(imgStream)
	return img

def processGAN(socket:socket.socket, transaction:int, image:Image):
    print('Start GAN')
    output = cartoonGAN(image)
    print('Complete GAN')
    imgBinary = image_to_byte_array(output)
    buffer = bytearray()
    payload = bytearray()
    buffer += int.to_bytes(PACKET_CODE,4,'little')

    payload += int.to_bytes(3,4,'little') # type
    payload += int.to_bytes(transaction,4,'little') #ID
    payload += int.to_bytes(len(imgBinary),4,'little') #img Length
    payload += imgBinary #img
    
    payloadLen = len(payload)
    buffer += int.to_bytes(payloadLen,4,'little') #payload Length
    buffer += payload
    
    socket.send(buffer)
    return

def recvProc(clientSocket):
	buffer= bytearray()
	while (isShutdown == False):
            data = clientSocket.recv(1024)
            if not data:
                break
            buffer += data
            # Packet Proc
            while True:
                if(len(buffer) < HEADER_SIZE):
                    break
                header = peek_buffer(buffer,HEADER_SIZE)

                magicNum = int.from_bytes(bytes(read_buffer(header,4)),'little')
                if(magicNum != PACKET_CODE):
                    print('packet code error : {}'.format(magicNum))
                    break
                payloadLength = int.from_bytes(bytes(read_buffer(header,4)),'little')
                if(len(buffer) < payloadLength + 5):
                    #print('packet too short {} < {} +5'.format(len(buffer),payloadLength))
                    break
                read_buffer(buffer,HEADER_SIZE)
                msgType = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                print('MsgType : {}'.format(msgType))
                transactionID = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                imgLength = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                print(imgLength)
                imgBinary = read_buffer(buffer, imgLength)
                srcImage = byte_array_to_image(bytes(imgBinary))
                thread = Thread(target=processGAN, args=(clientSocket,transactionID,srcImage))
                thread.start()
	
	clientSocket.close()
	return

#
#main
#

listenSocket.bind((HOST,PORT))
listenSocket.listen()
print('Cartoon GAN Server Start...')

while (isShutdown == False):
	#clientSock, addr = listenSocket.accept()
    result = listenSocket.accept()
    thread = Thread(target=recvProc, args=(result[0],))
    threadList.append(thread)
    thread.start()
	

for thread in threadList:
	thread.join()

listenSocket.close()
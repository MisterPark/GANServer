import sys
import numpy as np
import chainer
import six
import os

from cgi import parse_header, parse_multipart
from urllib.parse import parse_qs

sys.path.append('./cgi-bin/paint_x2_unet')
import cgi_exe
sys.path.append('./cgi-bin/helpers')
from platformAdapter import OSHelper

#import PIL
from PIL import Image

from chainer import cuda, serializers, Variable  # , optimizers, training
import cv2
import os.path

import unet
import lnet

import socket
from threading import Thread
from queue import Queue
import io


def cvt2YUV(img):
    (major, minor, _) = cv2.__version__.split(".")
    if major == '3':
        img = cv2.cvtColor( img, cv2.COLOR_RGB2YUV )
    else:
        img = cv2.cvtColor( img, cv2.COLOR_BGR2YUV )
    return img

class ImageAndRefDataset(chainer.dataset.DatasetMixin):

    def __init__(self, paths, root1='./input', root2='./ref', dtype=np.float32):
        self._paths = paths
        self._root1 = root1
        self._root2 = root2
        self._dtype = dtype

    def __len__(self):
        return len(self._paths)

    def get_name(self, i):
        return self._paths[i]

    def get_example(self, i, minimize=False, blur=0, s_size=128):
        path1 = os.path.join(self._root1, self._paths[i])
        #image1 = ImageDataset._read_image_as_array(path1, self._dtype)

def pil2cv(input:Image, type=cv2.COLOR_RGB2BGR):
    numpy_img = np.array(input)
    #output = cv2.cvtColor(numpy_img, type)
    output = numpy_img
    return output

def cv2pil(input):
    cv2_img = input
    #cv2_img = cv2.cvtColor(input, cv2.COLOR_BGR2RGB)
    output = Image.fromarray(cv2_img)
    return output

def get_example(i, input:Image,input_ref:Image, minimize=False, blur=0, s_size=128):
        #image1 = input.convert('RGB')
        image1 = pil2cv(input)
        image1 = cv2.cvtColor(image1, cv2.COLOR_RGB2GRAY)
        image1 = np.asarray(image1, np.float32)

        _image1 = image1.copy()
        if minimize:
            if image1.shape[0] < image1.shape[1]:
                s0 = s_size
                s1 = int(image1.shape[1] * (s_size / image1.shape[0]))
                s1 = s1 - s1 % 16
                _s0 = 4 * s0
                _s1 = int(image1.shape[1] * ( _s0 / image1.shape[0]))
                _s1 = (_s1+8) - (_s1+8) % 16
            else:
                s1 = s_size
                s0 = int(image1.shape[0] * (s_size / image1.shape[1]))
                s0 = s0 - s0 % 16
                _s1 = 4 * s1
                _s0 = int(image1.shape[0] * ( _s1 / image1.shape[1]))
                _s0 = (_s0+8) - (_s0+8) % 16

            _image1 = image1.copy()
            _image1 = cv2.resize(_image1, (_s1, _s0),
                                 interpolation=cv2.INTER_AREA)
            #noise = np.random.normal(0,5*np.random.rand(),_image1.shape).astype(self._dtype)

            if blur > 0:
                blured = cv2.blur(_image1, ksize=(blur, blur))
                image1 = _image1 + blured - 255

            image1 = cv2.resize(image1, (s1, s0), interpolation=cv2.INTER_AREA)

        # image is grayscale
        if image1.ndim == 2:
            image1 = image1[:, :, np.newaxis]
        if _image1.ndim == 2:
            _image1 = _image1[:, :, np.newaxis]

        image1 = np.insert(image1, 1, -512, axis=2)
        image1 = np.insert(image1, 2, 128, axis=2)
        image1 = np.insert(image1, 3, 128, axis=2)

        # add color ref image
        if minimize:
            image_ref = pil2cv(input_ref)
            image_ref = cv2.resize(image_ref, (image1.shape[1], image1.shape[
                                   0]), interpolation=cv2.INTER_NEAREST)
            b, g, r, a = cv2.split(image_ref)
            image_ref = cvt2YUV( cv2.merge((b, g, r)) )

            for x in range(image1.shape[0]):
                for y in range(image1.shape[1]):
                    if a[x][y] != 0:
                        for ch in range(3):
                            image1[x][y][ch + 1] = image_ref[x][y][ch]

        else:
            image_ref = pil2cv(input_ref,cv2.IMREAD_COLOR)
            image_ref = cvt2YUV(image_ref)
            image1 = cv2.resize(
                image1, (4 * image_ref.shape[1], 4 * image_ref.shape[0]), interpolation=cv2.INTER_AREA)
            image_ref = cv2.resize(image_ref, (image1.shape[1], image1.shape[
                                   0]), interpolation=cv2.INTER_AREA)

            image1[:, :, 1:] = image_ref

        return image1.transpose(2, 0, 1), _image1.transpose(2, 0, 1)

class Painter:

    def __init__(self, gpu=0):

        print("start")
        self.root = "./images/"
        self.batchsize = 1
        self.outdir = self.root + "out/"
        self.outdir_min = self.root + "out_min/"
        self.gpu = gpu
        self._dtype = np.float32

        if not os.path.isfile("./models/unet_128_standard"):
            print("./models/unet_128_standard not found. Please download them from http://paintschainer.preferred.tech/downloads/")
        if not os.path.isfile("./models/unet_512_standard"):
            print("./models/unet_512_standard not found. Please download them from http://paintschainer.preferred.tech/downloads/")

        print("load model")
        if self.gpu >= 0:
            cuda.get_device(self.gpu).use()
            cuda.set_max_workspace_size(64 * 1024 * 1024)  # 64MB
            chainer.Function.type_check_enable = False
        self.cnn_128 = unet.UNET()
        self.cnn_512 = unet.UNET()
        if self.gpu >= 0:
            self.cnn_128.to_gpu()
            self.cnn_512.to_gpu()
        #lnn = lnet.LNET()
        #serializers.load_npz("./cgi-bin/wnet/models/model_cnn_128_df_4", cnn_128)
        #serializers.load_npz("./cgi-bin/paint_x2_unet/models/model_cnn_128_f3_2", cnn_128)
        serializers.load_npz(
            "./models/unet_128_standard", self.cnn_128)
        #serializers.load_npz("./cgi-bin/paint_x2_unet/models/model_cnn_128_ua_1", self.cnn_128)
        #serializers.load_npz("./cgi-bin/paint_x2_unet/models/model_m_1.6", self.cnn)
        serializers.load_npz(
            "./models/unet_512_standard", self.cnn_512)
        #serializers.load_npz("./cgi-bin/paint_x2_unet/models/model_p2_1", self.cnn)
        #serializers.load_npz("./cgi-bin/paint_x2_unet/models/model_10000", self.cnn)
        #serializers.load_npz("./cgi-bin/paint_x2_unet/models/liner_f", lnn)

    def save_as_img(self, array, name):
        array = array.transpose(1, 2, 0)
        array = array.clip(0, 255).astype(np.uint8)
        array = cuda.to_cpu(array)
        (major, minor, _) = cv2.__version__.split(".")
        if major == '3':
            img = cv2.cvtColor(array, cv2.COLOR_YUV2RGB)
        else:
            img = cv2.cvtColor(array, cv2.COLOR_YUV2BGR)
        cv2.imwrite(name, img)

    def liner(self, id_str):
        if self.gpu >= 0:
            cuda.get_device(self.gpu).use()

        image1 = cv2.imread(path1, cv2.IMREAD_GRAYSCALE)
        image1 = np.asarray(image1, self._dtype)
        if image1.ndim == 2:
            image1 = image1[:, :, np.newaxis]
        img = image1.transpose(2, 0, 1)
        x = np.zeros((1, 3, img.shape[1], img.shape[2]), dtype='f')
        if self.gpu >= 0:
            x = cuda.to_gpu(x)

        lnn = lnet.LNET()
        with chainer.no_backprop_mode():
            with chainer.using_config('train', False):
                y = lnn.calc(Variable(x))

        self.save_as_img(y.data[0], self.root + "line/" + id_str + ".jpg")

    def colorize(self, img:Image,img_ref:Image, step='C', blur=0, s_size=128):
        if self.gpu >= 0:
            cuda.get_device(self.gpu).use()
        
        _ = {'S': True, 'L': False, 'C': True}

        sample = get_example(0,img,img_ref, minimize=_[step], blur=blur, s_size=s_size)
        _ = {'S': 0, 'L': 1, 'C': 0}[step]
        sample_container = np.zeros(
            (1, 4, sample[_].shape[1], sample[_].shape[2]), dtype='f')
        sample_container[0, :] = sample[_]

        if self.gpu >= 0:
            sample_container = cuda.to_gpu(sample_container)

        cnn = {'S': self.cnn_128, 'L': self.cnn_512, 'C': self.cnn_128}
        with chainer.no_backprop_mode():
            with chainer.using_config('train', False):
                image_conv2d_layer = cnn[step].calc(Variable(sample_container))
        del sample_container

        if step == 'C':
            input_bat = np.zeros((1, 4, sample[1].shape[1], sample[1].shape[2]), dtype='f')
            #print(input_bat.shape)
            input_bat[0, 0, :] = sample[1]

            output = cuda.to_cpu(image_conv2d_layer.data[0])
            del image_conv2d_layer  # release memory

            for channel in range(3):
                input_bat[0, 1 + channel, :] = cv2.resize(
                    output[channel, :], 
                    (sample[1].shape[2], sample[1].shape[1]), 
                    interpolation=cv2.INTER_CUBIC)

            if self.gpu >= 0:
                link = cuda.to_gpu(input_bat, None)
            else:
                link = input_bat
            with chainer.no_backprop_mode():
                with chainer.using_config('train', False):
                    image_conv2d_layer = self.cnn_512.calc(Variable(link))
            del link  # release memory
        
        array = image_conv2d_layer.data[0].transpose(1, 2, 0)
        array = array.clip(0, 255).astype(np.uint8)
        array = cuda.to_cpu(array)
        (major, minor, _) = cv2.__version__.split(".")
        if major == '3':
            img = cv2.cvtColor(array, cv2.COLOR_YUV2RGB)
        else:
            img = cv2.cvtColor(array, cv2.COLOR_YUV2BGR)
        #cv2.imwrite(image_out_path[step], img)
        pil_img = cv2pil(img)
        del image_conv2d_layer
        return pil_img


valid_ext = ['.jpg', '.png']
painter = Painter(gpu=0)

# const
HOST = '127.0.0.1'
PORT = 11111
PACKET_CODE = int('0x77',16)
HEADER_SIZE = 8

# global
jobQ = Queue()
listenSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
threadList = []
isShutdown = False

# function
def read_buffer(buffer:bytearray, count:int):
    if(len(buffer)<count):
        return None
    ret = buffer[0:count]
    del buffer[0:count]
    return ret

def peek_buffer(buffer:bytearray, count:int):
	if(len(buffer)< count):
		return None
	ret = buffer[0:count]
	return ret

def image_to_bytes(image:Image):
	imgByteArr = io.BytesIO()
	image.save(imgByteArr, 'PNG')
	imgByteArr = imgByteArr.getvalue()
	return imgByteArr

def bytes_to_image(binary:bytes):
	imgStream = io.BytesIO(binary)
	img = Image.open(imgStream)
	return img

def processGAN(socket:socket.socket, id:int, transactionId:int, image:Image, image_ref:Image):
    print('Start PaintsChainer : {}'.format(transactionId))
    output = painter.colorize(image, image_ref)
    print('Complete PaintsChainer : {}'.format(transactionId))
    imgBinary = image_to_bytes(output)
    buffer = bytearray()
    payload = bytearray()
    buffer += int.to_bytes(PACKET_CODE,4,'little')

    payload += int.to_bytes(6,4,'little') # type
    payload += int.to_bytes(id,4,'little') #ID
    payload += int.to_bytes(transactionId,4,'little') # transactionId
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
            data = clientSocket.recv(1024000)
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
                    print('packet too short {} < {} +5'.format(len(buffer),payloadLength))
                    break
                print('Packet Process...')
                read_buffer(buffer,HEADER_SIZE)
                msgType = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                #print('MsgType : {}'.format(msgType))
                id = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                transactionId = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                imgLength = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                imgBinary = read_buffer(buffer, imgLength)
                srcImage = bytes_to_image(bytes(imgBinary))
                imgLength2 = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                imgBinary2 = read_buffer(buffer, imgLength2)
                refImage = bytes_to_image(bytes(imgBinary2))
                thread = Thread(target=processGAN, args=(clientSocket,id,transactionId,srcImage,refImage))
                thread.start()
	
	clientSocket.close()
	return

#
#main
#

listenSocket.bind((HOST,PORT))
listenSocket.listen()
print('PaintsChainer Server Start...')

while (isShutdown == False):
	#clientSock, addr = listenSocket.accept()
    result = listenSocket.accept()
    result[0].setsockopt(socket.IPPROTO_TCP,socket.TCP_NODELAY,1)
    thread = Thread(target=recvProc, args=(result[0],))
    threadList.append(thread)
    thread.start()
	

for thread in threadList:
	thread.join()

listenSocket.close()
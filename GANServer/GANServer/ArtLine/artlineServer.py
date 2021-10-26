import warnings

warnings.filterwarnings(action='ignore')
import numpy as np
import argparse
import torch
from torchvision import transforms
import os.path
import fastai
from fastai.vision import *
from fastai.utils.mem import *
from fastai.vision import open_image, load_learner, image, torch
import urllib.request
import torchvision.transforms as T
import requests
import cv2
import time

import PIL
import socket
from threading import Thread
from queue import Queue
import io

class FeatureLoss(nn.Module):
    def __init__(self, m_feat, layer_ids, layer_wgts):
        super().__init__()
        self.m_feat = m_feat
        self.loss_features = [self.m_feat[i] for i in layer_ids]
        self.hooks = hook_outputs(self.loss_features, detach=False)
        self.wgts = layer_wgts
        self.metric_names = ['pixel',] + [f'feat_{i}' for i in range(len(layer_ids))
              ] + [f'gram_{i}' for i in range(len(layer_ids))]

    def make_features(self, x, clone=False):
        self.m_feat(x)
        return [(o.clone() if clone else o) for o in self.hooks.stored]
    
    def forward(self, input, target):
        out_feat = self.make_features(target, clone=True)
        in_feat = self.make_features(input)
        self.feat_losses = [base_loss(input,target)]
        self.feat_losses += [base_loss(f_in, f_out)*w
                             for f_in, f_out, w in zip(in_feat, out_feat, self.wgts)]
        self.feat_losses += [base_loss(gram_matrix(f_in), gram_matrix(f_out))*w**2 * 5e3
                             for f_in, f_out, w in zip(in_feat, out_feat, self.wgts)]
        self.metrics = dict(zip(self.metric_names, self.feat_losses))
        return sum(self.feat_losses)
    
    def __del__(self): self.hooks.remove()

# initialize
print('**** Initialize... ****')
CurDir = os.path.dirname(os.path.realpath(__file__))
learn=load_learner(Path("."), CurDir + '/checkpoint/ArtLine_650.pkl')
if learn is None:
    print('Error : learn is none!!')
    print('**** Initialize Failed ****')
    exit()
else:
    print('**** Complete Initialize ****')

def Artline(input:PIL.Image):
    # pil to cv2
    numpy_image = np.array(input)
    image = cv2.cvtColor(numpy_image,cv2.COLOR_RGB2BGR)
    # Artline Processing
    p,img_hr,b = learn.predict(Image(T.ToTensor()(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))))
    output = np.uint8(np.clip(image2np(img_hr), 0, 1) * 255)
    #cv2 to pil
    cv2_image = cv2.cvtColor(output, cv2.COLOR_BGR2RGB)
    output_image = PIL.Image.fromarray(cv2_image)
    return output_image




# const
HOST = '127.0.0.1'
PORT = 6666
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

def image_to_bytes(image:PIL.Image):
	imgByteArr = io.BytesIO()
	image.save(imgByteArr, 'JPEG')
	imgByteArr = imgByteArr.getvalue()
	return imgByteArr

def bytes_to_image(binary:bytes):
	imgStream = io.BytesIO(binary)
	img = PIL.Image.open(imgStream)
	return img

def processGAN(socket:socket.socket, id:int, transactionId:int, image:PIL.Image):
    print('Start Artline : {}'.format(transactionId))
    output = Artline(image)
    print('Complete Artline : {}'.format(transactionId))
    imgBinary = image_to_bytes(output)
    buffer = bytearray()
    payload = bytearray()
    buffer += int.to_bytes(PACKET_CODE,4,'little')

    payload += int.to_bytes(5,4,'little') # type
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
                    #print('packet too short {} < {} +5'.format(len(buffer),payloadLength))
                    break
                read_buffer(buffer,HEADER_SIZE)
                msgType = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                #print('MsgType : {}'.format(msgType))
                id = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                transactionId = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                imgLength = int.from_bytes(bytes(read_buffer(buffer,4)),'little')
                #print(imgLength)
                imgBinary = read_buffer(buffer, imgLength)
                srcImage = bytes_to_image(bytes(imgBinary))
                thread = Thread(target=processGAN, args=(clientSocket,id,transactionId,srcImage))
                thread.start()
	
	clientSocket.close()
	return

#
#main
#

listenSocket.bind((HOST,PORT))
listenSocket.listen()
print('Artline Server Start...')

while (isShutdown == False):
    result = listenSocket.accept()
    result[0].setsockopt(socket.IPPROTO_TCP,socket.TCP_NODELAY,1)
    thread = Thread(target=recvProc, args=(result[0],))
    threadList.append(thread)
    thread.start()
	

for thread in threadList:
	thread.join()

listenSocket.close()
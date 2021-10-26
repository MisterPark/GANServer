import os
import cv2
import numpy as np
import tensorflow as tf 
import network
import guided_filter
from tqdm import tqdm

from PIL import Image
import socket
from threading import Thread
from queue import Queue
import io


def resize_crop(image):
    h, w, c = np.shape(image)
    if min(h, w) > 720:
        if h > w:
            h, w = int(720*h/w), 720
        else:
            h, w = 720, int(720*w/h)
    image = cv2.resize(image, (w, h),
                       interpolation=cv2.INTER_AREA)
    h, w = (h//8)*8, (w//8)*8
    image = image[:h, :w, :]
    return image

# initialize
input_photo = tf.placeholder(tf.float32, [1, None, None, 3])
network_out = network.unet_generator(input_photo)
final_out = guided_filter.guided_filter(input_photo, network_out, r=1, eps=5e-3)

all_vars = tf.trainable_variables()
gene_vars = [var for var in all_vars if 'generator' in var.name]
saver = tf.train.Saver(var_list=gene_vars)
    
config = tf.ConfigProto()
config.gpu_options.allow_growth = True
sess = tf.Session(config=config)

sess.run(tf.global_variables_initializer())
saver.restore(sess, tf.train.latest_checkpoint('saved_models'))

def cartoonize(input:Image):

    try:
        # pil to cv2
        numpy_image = np.array(input)
        image = cv2.cvtColor(numpy_image, cv2.COLOR_RGB2BGR)

        image = resize_crop(image)
        batch_image = image.astype(np.float32)/127.5 - 1
        batch_image = np.expand_dims(batch_image, axis=0)
        output = sess.run(final_out, feed_dict={input_photo: batch_image})
        output = (np.squeeze(output)+1)*127.5
        output = np.clip(output, 0, 255).astype(np.uint8)

        #cv2 to pil
        cv2_image = cv2.cvtColor(output, cv2.COLOR_BGR2RGB)
        output_image = Image.fromarray(cv2_image)
        return output_image
    except:
        print('cartoonize failed')
        



# const
HOST = '127.0.0.1'
PORT = 8888
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
	image.save(imgByteArr, 'JPEG')
	imgByteArr = imgByteArr.getvalue()
	return imgByteArr

def bytes_to_image(binary:bytes):
	imgStream = io.BytesIO(binary)
	img = Image.open(imgStream)
	return img

def processGAN(socket:socket.socket, id:int, transactionId:int, image:Image):
    print('Start Cartoonize : {}'.format(transactionId))
    output = cartoonize(image)
    print('Complete Cartoonize : {}'.format(transactionId))
    imgBinary = image_to_bytes(output)
    buffer = bytearray()
    payload = bytearray()
    buffer += int.to_bytes(PACKET_CODE,4,'little')

    payload += int.to_bytes(4,4,'little') # type
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
print('Cartoonize Server Start...')

while (isShutdown == False):
    result = listenSocket.accept()
    result[0].setsockopt(socket.IPPROTO_TCP,socket.TCP_NODELAY,1)
    thread = Thread(target=recvProc, args=(result[0],))
    threadList.append(thread)
    thread.start()
	

for thread in threadList:
	thread.join()

listenSocket.close()
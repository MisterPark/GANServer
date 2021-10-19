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
from io import BytesIO
import torchvision.transforms as T
import requests
import PIL
import cv2

import time

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

if __name__ == '__main__':
    print('**** Start ArtLine ****')
    CurDir = os.path.dirname(os.path.realpath(__file__))
    learn=load_learner(Path("."), CurDir + '/checkpoint/ArtLine_650.pkl')
    if learn is None:
        print('learn is none')
    else:
        print('Checkpoint load complete')
        FolderDir = CurDir + '/translate/'
        ResultFolderDir = CurDir + '/results/'
        Files = os.listdir(FolderDir)

        ImageList = []
        for fileName in Files:
            ImageList.append(cv2.imread(FolderDir + fileName, flags=cv2.IMREAD_COLOR))
        
        imgIndex = 0
        for imgFile in ImageList:
            print('Processing: ' + Files[imgIndex])
            p,img_hr,b = learn.predict(Image(T.ToTensor()(cv2.cvtColor(imgFile, cv2.COLOR_BGR2RGB))))
            cv2.imwrite(ResultFolderDir +  Files[imgIndex], np.uint8(np.clip(image2np(img_hr), 0, 1) * 255))
            imgIndex += 1
        imgIndex = 0
        print('**** Translate End ****')
    print('**** End ArtLine ****')

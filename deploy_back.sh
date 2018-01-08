#!/bin/bash
sudo service jadeflix stop
#set -x
sudo rm -rf /home/pi/services/jadeflix/*
sudo cp -R /home/pi/builds/jadeflix/back/* /home/pi/services/jadeflix/
sudo chmod a+x /home/pi/services/jadeflix/JadeFlix
sudo rm -rf /home/pi/builds/jadeflix/back/*
sleep 10

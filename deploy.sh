#!/bin/bash
sudo service jadeflix stop
#set -x
sudo rm -rf /home/pi/services/jadeflix/*
sudo cp -R /home/pi/builds/jadeflix/back/* /home/pi/services/jadeflix/
sudo rm -rf /home/pi/builds/jadeflix/back/*
sudo chmod a+x /home/pi/services/jadeflix/JadeFlix
sudo rm -rf /var/www/jadeflix/*.css
sudo rm -rf /var/www/jadeflix/*.js
sudo rm -rf /var/www/jadeflix/*.html
sudo rm -rf /var/www/jadeflix/*.txt
sudo rm -rf /var/www/jadeflix/*.ico
sudo cp -R /home/pi/builds/jadeflix/front/* /var/www/jadeflix/
sudo rm -rf /home/pi/builds/jadeflix/front/*
sleep 10
sudo service jadeflix start

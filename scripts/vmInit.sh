#!/bin/bash

# Open ports in ubuntu, ports need to be opened also in Azure nsg
sudo ufw allow  5000, 80

# install .NET 5 sdk via snap
sudo snap install dotnet-sdk --classic --channel=5.0
sudo snap alias dotnet-sdk.dotnet dotnet

#install nginx
sudo apt update 
sudo apt install nginx
# open firewall port 80
sudo ufw allow 'Nginx HTTP'
#Restart nginx, folder for files is /var/wwww/html
sudo systemctl restart nginx

#install vsftpd
sudo apt install vsftpd
# restart vsftpd
sudo service vsftpd restart
# Add folder for ftps certs
sudo mkdir /etc/ssl/vsftpd
# create self signed certs
sudo openssl req -x509 -nodes -days 365 -newkey rsa:1024 -keyout /etc/ssl/vsftpd/vsftpd.pem -out /etc/ssl/vsftpd/vsftpd.pem

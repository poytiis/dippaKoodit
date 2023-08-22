from ftplib import FTP, FTP_TLS
import os
import paramiko
from datetime import datetime
import json
import random

SERVER_IP = '52.190.63.78'

def ftp_upload(file_path: str, file_name: str):
    file_size = os.path.getsize(file_path)
    start_time = datetime.now()

    with FTP(SERVER_IP, 'anonymous', 'anonymous') as ftp:
        ftp.cwd('upload')
        with open(file_path, 'rb') as image_file:
            ftp.storbinary('STOR %s' % generate_random_file_name(file_name), image_file)
            end_time = datetime.now()
            upload_time = (end_time - start_time).total_seconds()

    return 'FTP,' + file_name + ',' + str(file_size) + ',' + str(upload_time)  + ',Python\n'

def ftps_upload(file_path: str, file_name: str):
    file_size = os.path.getsize(file_path)
    start_time = datetime.now()

    with FTP_TLS(SERVER_IP, 'anonymous', 'anonymous') as ftps:
        ftps.prot_p()
        ftps.cwd('upload')
        with open(file_path, 'rb') as image_file:
            ftps.storbinary('STOR %s' %  generate_random_file_name(file_name), image_file)
            end_time = datetime.now()
            upload_time = (end_time - start_time).total_seconds()

    return 'FTPS,' + file_name + ',' + str(file_size) + ',' + str(upload_time)  + ',Python\n'

def sftp_upload(file_path: str, file_name: str):
    file_size = os.path.getsize(file_path)
    with open('secrets.json') as file:
        secrets = json.load(file)
        password = secrets['sftpPass']
        
    start_time = datetime.now()

    client = paramiko.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(SERVER_IP, username='poytiis', password=password)

    sftp = client.open_sftp()
    sftp.put(file_path, './sftp/' +  generate_random_file_name(file_name))
   
    end_time = datetime.now()
    upload_time = (end_time - start_time).total_seconds()

    sftp.close()
    client.close()

    return 'SFTP,' + file_name + ',' + str(file_size) + ',' + str(upload_time)  + '\n'

def calc_avarage_times(upload_times: int):
    with open('ftpUploadTimes.csv') as file:
        file_lines = file.readlines()
    
    avarage_dict = {}
    for line in file_lines:
        line_split = line.strip().split(',')
        if line_split[0] == 'protocol':
            continue
        line_key = line_split[0] + ',' + line_split[1] + ',' + line_split[2]
        line_time = float(line_split[3])
        if line_key in avarage_dict:
            avarage_dict[line_key] = avarage_dict[line_key] + line_time
        else:
            avarage_dict[line_key] = line_time

    with open('ftpUploadTimesAverage.csv', 'w') as file:
        for avarage_key in avarage_dict:
            avarage_dict[avarage_key] = round(avarage_dict[avarage_key] / upload_times, 2)
            file.write(avarage_key + ',' + str(avarage_dict[avarage_key]) + ',Python\n')
        
def generate_random_file_name(file_name: str):
    random_number = str(random.randint(0, 10000000))
    file_name_split = file_name.split('.')
    return file_name_split[0] + random_number + '.' + file_name_split[1]

def main():
    file_paths = ['D:/dippa/uploadData/kilo.txt', 'D:/dippa/uploadData/mega.txt', 'D:/dippa/uploadData/tenMega.txt']
    file_names = ['kilo.txt', 'mage.txt', 'tenMega.txt']

    upload_times = 5

    if os.path.exists("ftpUploadTimes.csv"):
        os.remove("ftpUploadTimes.csv")

    init_csv_row = 'protocol,file_name,file_size,upload_time,client\n'
    with open('ftpUploadTimes.csv', 'w') as file:
       file.write(init_csv_row)

    csv_rows = ''

    for j in range(len(file_paths)):

        for i in range (0, upload_times):   
            row = ftp_upload(file_paths[j], file_names[j])
            csv_rows += row

        for i in range (0, upload_times):   
            row = ftps_upload(file_paths[j], file_names[j])
            csv_rows += row

        for i in range (0, upload_times):   
            row = sftp_upload(file_paths[j], file_names[j])
            csv_rows += row

    with open('ftpUploadTimes.csv', 'a') as file:
        file.write(csv_rows)

    calc_avarage_times(upload_times)
    

main()


          
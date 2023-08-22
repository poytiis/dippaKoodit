from pydantic import FilePath
from selenium import webdriver
from selenium.webdriver.common.keys import Keys
import time
import os
from selenium.webdriver.firefox.webdriver import WebDriver

def select_settings(csv_rows, driver: WebDriver, protocol: str, format: str, body_size: int, file_name: str):
    if protocol == 'websocket':
        protocol_radio_class = 'upload__radio-websocket'
        format_radio_class = 'upload__radio-radiowebsocket-' + format
        body_size_class = None
    else:
        protocol_radio_class = 'upload__radio-http'
        format_radio_class = 'upload__radio-http-' + format
        body_size_class = 'upload__radio-http-' + body_size

    protocol_elements = driver.find_elements_by_class_name(protocol_radio_class)
    protocol_elements[0].click()
    time.sleep(2)
    format_elements = driver.find_elements_by_class_name(format_radio_class)
    format_elements[0].click()

    if body_size_class != None:
        body_size_elements = driver.find_elements_by_class_name(body_size_class)
        body_size_elements[0].click()

    file_path = "D:\\dippa\\uploadData\\" + file_name
    driver.find_elements_by_class_name('upload__file-input')[0].send_keys(file_path)

    driver.find_element_by_id('upload__upload-button').click()
    # time.sleep(5)

    upload_time = driver.find_element_by_id('upload__result-time').text


    file_size = os.path.getsize(file_path)
    csv_row = protocol + ',' + format + ',' + body_size + ',' + file_name + ',' + upload_time + ',' +  str(file_size) + '\n' 
    csv_rows.append(csv_row)

def calc_avarage_times(upload_times: int):
    with open('httpUploadTimes.csv') as file:
        file_lines = file.readlines()
    
    avarage_dict = {}
    for line in file_lines:
        line_split = line.strip().split(',')
        if line_split[0] == 'protocol':
            continue
        line_key = line_split[0] + ',' + line_split[1]+ ',' + line_split[2]+ ',' + line_split[3] + ',' + line_split[5]
        line_time = float(line_split[4])
        if line_key in avarage_dict:
            avarage_dict[line_key] = avarage_dict[line_key] + line_time
        else:
            avarage_dict[line_key] = line_time

    f = open('httpUploadTimesAverage.csv', 'w')
    for avarage_key in avarage_dict:
        avarage_dict[avarage_key] = round(avarage_dict[avarage_key] / upload_times, 2)
        f.write(avarage_key + ',' + str(avarage_dict[avarage_key]) + '\n')

    f.close()

def main():
    driver = webdriver.Firefox(service_log_path=os.path.devnull)
    driver.implicitly_wait(600)

    if os.path.exists("httpUploadTimes.csv"):
        os.remove("httpUploadTimes.csv")
    init_csv_row = 'protocol,format,body_size,file_name,upload_time,file_size\n'

    with open('httpUploadTimes.csv', 'w') as file:
       file.write(init_csv_row)
        
    driver.get('http://20.55.51.145/')

    upload_times_with_same_params = 1
    http_formats = ['base64', 'array', 'form-data']
    http_body_sizes = ['5kb', '50kb', '500kb']
    file_names = ['kilo.txt', 'mega.txt']
    csv_rows = []

    for http_format in http_formats:
        for http_body_size in http_body_sizes:
            for file_name in file_names:
                for i in range(0, upload_times_with_same_params):
                    try:
                        select_settings(csv_rows, driver, 'HTTP', http_format, http_body_size, file_name)
                    except:
                        pass

    websocket_formats = ['string', 'binary']

    # for websocket_format in websocket_formats:
    #     for file_name in file_names:
    #         for i in range(0, upload_times_with_same_params):
    #             select_settings(csv_rows, driver, 'websocket', websocket_format, '', file_name)
   
    driver.close()

    with open('httpUploadTimes.csv', 'a') as file:
        file.write(''.join(csv_rows))

    calc_avarage_times(upload_times_with_same_params)


main()
 
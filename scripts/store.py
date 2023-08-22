import requests

def main():
    upload_times = 2 
    result_file_lines = []
    avarage_dict = {}

    res = requests.get('https://localhost:44376/api/Store/RunStoreComparison', verify=False)
    csv_lines = res.text.split('\n')

    for line in csv_lines:
        line = line.strip()
        result_file_lines.append(line)
        line_split = line.split(',')
        if line_split[0] == 'storeType' or line_split[0] == '':
            continue
        line_key = line_split[0] + ',' + line_split[3]
        line_time = float(line_split[2])
        if line_key in avarage_dict:
            avarage_dict[line_key] = avarage_dict[line_key] + line_time
        else:
            avarage_dict[line_key] = line_time

    result_file_text = '\n'.join(result_file_lines)
    with open('storeTimes.csv', 'w') as result_file:
        result_file.write(result_file_text)

    with open('storeTimesAverage.csv', 'w') as result_avarage_file:
        for avarage_key in avarage_dict:
            avarage_dict[avarage_key] = round(avarage_dict[avarage_key] / upload_times, 4)
            result_avarage_file.write(avarage_key + ',' + str(avarage_dict[avarage_key]) + '\n')

main()
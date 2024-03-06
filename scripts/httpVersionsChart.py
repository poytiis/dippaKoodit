import matplotlib.pyplot as plt
import numpy as np
from datetime import datetime

def read_csv_file(file_path: str):
  with open(file_path, 'r') as file:
    lines = file.readlines()

  results = {}
  for line in lines:
    line_split = line.split(',')
    version = line_split[2]
    if version == 'HTTPVersion':
      continue
    file_size = line_split[4].strip()
    time_object = datetime.strptime(line_split[3].strip(), '%H:%M:%S').time()
    # k = time_object.total_seconds()
    # print(time_object)
    upload_time = time_object.minute * 60 + time_object.second
    # upload_time = float(line_split[3].strip())
    dict_key = version

    if dict_key in results:
      results[dict_key][file_size] = upload_time
    else:
      results[dict_key] = {file_size: upload_time}
  return results


def main():
  csv_files = ['HTTPVersionsAvarage.csv']
  csv_data = []
  for file in csv_files:
    csv_results = read_csv_file(file)
    csv_data.append(csv_results)

  bar_values = []
  bar_labels = ('1.1', '2', '3')
  bar_width = 0.25

  print(csv_data)
  for key in csv_data[0]:
    upload_times = list(csv_data[0][key].values())
    bar_values.append(upload_times)

  print(bar_values)

  X = np.arange(2)
  pl1 = plt.bar(X, bar_values[0], color = 'b', width = 0.25)
  pl2 = plt.bar(X + bar_width, bar_values[1], color = 'g', width = 0.25)
  pl3 = plt.bar(X + bar_width * 2, bar_values[2], color = 'r', width = 0.25)

  plt.xlabel('Tiedoston koko (MB)',  fontsize = 14)
  plt.ylabel('latausaika (s)', fontsize = 14)
  plt.xticks([r + bar_width for r in range(2)],
          ['100', '300'])
  plt.title('HTTP-lataus eri HTTP-versioilla')
  plt.legend((pl1[0], pl2[0], pl3[0]), bar_labels)
  plt.savefig('httpVersionsResults.png')

main()
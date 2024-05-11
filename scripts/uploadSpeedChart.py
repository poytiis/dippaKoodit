import matplotlib.pyplot as plt
import numpy as np

def read_csv_file(file_path: str):
  with open(file_path, 'r') as file:
    lines = file.readlines()

  results = {}
  for line in lines:
    line_split = line.split(',')
    protocol = line_split[0]
    time = float(line_split[3].strip())
    dict_key = protocol
    results[dict_key] = {'time': time}
  return results


def main():
  csv_files = ['uploadSpeedTimes.csv']
  csv_data = []
  for file in csv_files:
    csv_results = read_csv_file(file)
    csv_data.append(csv_results)

  bar_values = []
  bar_labels = ('1.1', '2', '3')
  bar_width = 0.25

  for key in csv_data[0]:
    print(key)
    bar_values.append(csv_data[0][key]['time'])

  print(csv_data)
  print(bar_values)
  # return
  X = np.arange(1)
  pl1 = plt.bar(X + bar_width, bar_values[0], color = 'b', width = 0.35)
  pl2 = plt.bar(X + bar_width + 1, bar_values[1], color = 'g', width = 0.35)
  pl2 = plt.bar(X + bar_width + 2, bar_values[2], color = 'r', width = 0.35)
  pl2 = plt.bar(X + bar_width + 3, bar_values[3], color = '#123123', width = 0.35)
  pl2 = plt.bar(X + bar_width + 4, bar_values[4], color = '#fff123', width = 0.35)
  pl2 = plt.bar(X + bar_width + 5, bar_values[5], color = '#aaa123', width = 0.35)
  pl2 = plt.bar(X + bar_width + 6, bar_values[6], color = '#60a123', width = 0.35)


  plt.xlabel('Protokolla',  fontsize = 14)
  plt.ylabel('aika (s)', fontsize = 14)
  plt.xticks([r + bar_width for r in range(7)],
          ['HTTP', 'WebSocket', 'FTP', 'FTPS', 'SFTP', 'HTTPS', 'WebDav'])
  plt.title('Tiedoston siirtoaika eri tiedonsiirtoprotokollilla')

  plt.savefig('uploadSpeedResults.png')
  

main()
import matplotlib.pyplot as plt
import numpy as np

def read_csv_file(file_path: str):
  with open(file_path, 'r') as file:
    lines = file.readlines()

  results = {}
  for line in lines:
    line_split = line.split(',')
    encoding = line_split[1]
    if encoding == 'protocol':
      continue
    body_size = line_split[2]
    upload_time = float(line_split[5].strip())
    # client = line_split[3].strip()
    dict_key = encoding # + ', ' + body_size

    if dict_key in results:
      results[dict_key][body_size] = upload_time
    else:
      results[dict_key] = {body_size: upload_time}
  return results


def main():
  csv_files = ['httpUploadTimesAverage.csv']
  csv_data = []
  for file in csv_files:
    csv_results = read_csv_file(file)
    csv_data.append(csv_results)

  bar_values = []
  bar_labels = ('base64', 'array', 'form-data')
  bar_width = 0.25

  for key in csv_data[0]:
    upload_times = list(csv_data[0][key].values())
    bar_values.append(upload_times)

  X = np.arange(3)
  pl1 = plt.bar(X, bar_values[0], color = 'b', width = 0.25)
  pl2 = plt.bar(X + bar_width, bar_values[1], color = 'g', width = 0.25)
  pl3 = plt.bar(X + bar_width * 2, bar_values[2], color = 'r', width = 0.25)

  plt.xlabel('HTTP-pyynnön koko (kB)',  fontsize = 14)
  plt.ylabel('latausaika (s)', fontsize = 14)
  plt.xticks([r + bar_width for r in range(len(bar_labels))],
          ['5', '50', '500'])
  plt.title('HTTP-lataus eri pyyntöjen koolla ja koodauksilla')
  plt.legend((pl1[0], pl2[0], pl3[0]), bar_labels)
  plt.savefig('httpUploadResults.png')

main()
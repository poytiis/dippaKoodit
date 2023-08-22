import matplotlib.pyplot as plt
import numpy as np

def read_csv_file(file_path: str):
  with open(file_path, 'r') as file:
    lines = file.readlines()

  results = {}
  for line in lines:
    line_split = line.split(',')
    operation = line_split[0]
    if operation == 'operation':
      continue
    type = line_split[1].strip()
    upload_time = float(line_split[2].strip())
    file_size = line_split[3].strip()

    dict_key = operation
    payload = {file_size: [{'type': type, 'upload_time': upload_time}]}
    # payload = { file_size}

    if dict_key in results:
      # print(results)
      results[dict_key][file_size].append({'type': type, 'upload_time': upload_time})
    else:
      results[dict_key] = payload
  return results

def main():
  results = read_csv_file('./SQLStoreTimes.csv')
  # print(results)

  bar_values = []
  bar_labels = ('link', 'blob')
  bar_width = 0.25
  blob_times = []
  link_times = []
  label_names = []
  file_size_print = ''

  for result in results:
    label_names.append(result)
    for file_size in results[result]:
      file_size_print = file_size
      data = results[result][file_size]
      print(data)
      for mesurment in data:
        if mesurment['type'] == 'blob':
          blob_times.append(mesurment['upload_time'])
        else:
          link_times.append(mesurment['upload_time'])

  print(blob_times)
  print(link_times)
  print(label_names)

  X = np.arange(3)
  pl1 = plt.bar(X, link_times, color = 'b', width = 0.25)
  pl2 = plt.bar(X + bar_width, blob_times, color = 'g', width = 0.25)

  plt.xlabel('Operaatio',  fontsize = 14)
  plt.ylabel('tallennusaika (s)', fontsize = 14)
  plt.xticks([r + bar_width for r in range(3)], label_names)     
  plt.title('Tiedostojen tallentaminen SQL kantaan tai levylle vertailu')
  plt.legend((pl1[0], pl2[0]), bar_labels)
  plt.savefig('SQLStoreResults.png')



main()



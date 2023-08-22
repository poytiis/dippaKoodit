import matplotlib.pyplot as plt
import numpy as np

def read_csv_file(file_path: str):
  with open(file_path, 'r') as file:
    lines = file.readlines()

  results = {}
  for line in lines:
    line_split = line.split(',')
    protocol = line_split[0]
    if protocol == 'protocol':
      continue
    format = line_split[1].strip()
    body_size = line_split[2].strip()
    increase_percent = float(line_split[6].strip())

    dict_key = format
    payload =  [{'body_size': body_size, 'increase_percent': increase_percent}]

    if dict_key in results:
      results[dict_key].append({'body_size': body_size, 'increase_percent': increase_percent})
    else:
      results[dict_key] = payload
  return results

def main():
  results = read_csv_file('./wiresharkHTTP.csv')
  print(results)


  bar_values = []
  bar_labels = ('5kb', '50kb', '500kb')
  bar_width = 0.2
  label_names = []
  file_size_print = ''
  times_5kb = []
  times_50kb = []
  times_500kb = []

  for result in results:
    label_names.append(result)
    for upload_object in results[result]:
      percent = upload_object['increase_percent']
      body_size = upload_object['body_size']
      print(body_size)
      if body_size == '5kb':
        times_5kb.append(percent)
      elif body_size == '50kb':
        times_50kb.append(percent)
      else:
        times_500kb.append(percent)
  print(times_5kb)
  print(times_50kb)
  print(times_500kb)
  print(label_names)

  br1 = np.arange(len(times_5kb))
  br2 = [x + bar_width for x in br1]
  br3 = [x + bar_width for x in br2]


  X = np.arange(3)
  pl1 = plt.bar(br1, times_5kb, color = 'b', width = bar_width)
  pl2 = plt.bar(br2, times_50kb, color = 'g', width = bar_width)
  pl3 = plt.bar(br3, times_500kb, color = 'r', width = bar_width)

  plt.xlabel('Operaatio',  fontsize = 14)
  plt.ylabel('tallennusaika (s)', fontsize = 14)
  plt.xticks([r + bar_width for r in range(3)], label_names)     
  plt.title('Tiedostojen tallentaminen SQL kantaan tai levylle vertailu')
  plt.legend((pl1[0], pl2[0], pl3[0]), bar_labels)
  plt.savefig('wiresharkHTTPResults.png')



main()



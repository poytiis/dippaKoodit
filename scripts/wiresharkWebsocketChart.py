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
    increase_percent = float(line_split[5].strip())

    dict_key = format
    payload =  [{'increase_percent': increase_percent}]

    if dict_key in results:
      results[dict_key].append({'increase_percent': increase_percent})
    else:
      results[dict_key] = payload
  return results

def main():
  results = read_csv_file('./wiresharkWebsocket.csv')
  print(results)


  bar_values = []
  bar_labels = ('binary', 'string')
  bar_width = 0.25
  label_names = []
  binary = []
  string = []

  for result in results:
    label_names.append(result)
    for upload_object in results[result]:
      percent = upload_object['increase_percent']
      if result == 'binary':
        binary.append(percent)
      else:
        string.append(percent)
  print(binary)
  print(string)
  print(label_names)

  br1 = np.arange(len(string))
  br1
  br2 = [x + bar_width  * 2 for x in br1]
  print(br1)
  print(br2)

  br1 = [0]
  br2 = [0.6]

  pl1 = plt.bar(br1, binary, color = 'b', width = bar_width )
  pl2 = plt.bar(br2, string, color = 'g', width = bar_width)

  plt.xlabel('protokolla',  fontsize = 14)
  plt.ylabel('tiedoston kasvu(%)', fontsize = 14)
  plt.xticks([0, 0.6], bar_labels)     
  plt.title('Websocket lähetyksen tiedoston kasvu')
  plt.legend((pl1[0], pl2[0]), bar_labels)
  plt.savefig('wiresharkWebsocketResults.png')



main()



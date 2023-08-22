import matplotlib.pyplot as plt
import numpy as np

def read_csv_file(file_path: str):
  with open(file_path, 'r') as file:
    lines = file.readlines()

  results = {}
  for line in lines:
    line_split = line.split(',')
    storage = line_split[0]
    if storage == 'protocol':
      continue
    file_size = line_split[1]
    upload_time = float(line_split[2].strip())

    dict_key = storage 

    if dict_key in results:
      results[dict_key][file_size] = upload_time
    else:
      results[dict_key] = {file_size: upload_time}
  return results


def main():
  csv_files = ['storeTimesAverage.csv']
  csv_data = []
  for file in csv_files:
    csv_results = read_csv_file(file)
    csv_data.append(csv_results)


  print(csv_data)

  for key in csv_data[0]:
    values = csv_data[0][key].values()
    keys = csv_data[0][key].keys()
    plt.plot(keys, values, label=key)


  plt.legend()
  plt.yscale("log")
  plt.title('Tiedostojen talletusnopeuksia')
  plt.xlabel('tiedoston koko (tavu)')
  plt.ylabel('aika (s)')
  plt.savefig('storeResults.png')

main()
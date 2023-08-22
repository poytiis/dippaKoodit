import matplotlib.pyplot as plt

def read_csv_file(file_path: str):
  with open(file_path, 'r') as file:
    lines = file.readlines()

  results = {}
  for line in lines:
    line_split = line.split(',')
    protocol = line_split[0]
    if protocol == 'protocol':
      continue
    file_size = float(line_split[2])
    upload_time = float(line_split[3].strip())
    client = line_split[4].strip()
    dict_key = protocol + ', ' + client

    if dict_key in results:
      results[dict_key][file_size] = upload_time
    else:
      results[dict_key] = {file_size: upload_time}
  return results

def main():
  csv_files = ['ftpUploadTimesAverage.csv', 'ftpUploadTimesAveargeC#.csv']
  csv_data = []
  for file in csv_files:
    csv_results = read_csv_file(file)
    csv_data.append(csv_results)

  print(csv_data)

  y_axis_lim = [0, 25]

  fig, (ax1, ax2) = plt.subplots(1, 2)

  for result in csv_data[0]:
    keys = csv_data[0][result].keys()
    values = csv_data[0][result].values() 
    ax1.plot(keys, values, label=result )

  ax1.set_title('FTP, FTPS ja SFTP lataukset Python koodi')
  ax1.set(xlabel='Tiedoston koko (tavu)', ylabel='Latausaika (s)')
  ax1.set_ylim(y_axis_lim)
  ax1.legend()

  for result in csv_data[1]:
    keys = csv_data[1][result].keys()
    values = csv_data[1][result].values()
    ax2.plot(keys, values, label=result )

  ax2.set_title('FTP, FTPS ja SFTP lataukset C# koodi')
  ax2.set(xlabel='Tiedoston koko (tavu)', ylabel='Latausaika (s)')
  ax2.set_ylim(y_axis_lim)
  ax2.legend()

  fig.set_size_inches(12, 6)
  fig.savefig('results.png')
    
main()
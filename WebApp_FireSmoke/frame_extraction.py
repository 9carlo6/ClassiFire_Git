#!flask/bin/python
from flask import Flask
app = Flask(__name__)
from flask import Flask
from flask import request
app = Flask(__name__)
@app.route('/postjson', methods=['POST'])
def post():
    print(request.is_file)
    content = request.get_file()
    #print(content)
    #print(content['id'])
    #print(content['name'])
    return 'JSON posted'

def post():
    print(request.is_json)
    content = request.get_json()
    #print(content)
    print(content['id'])
    print(content['name'])
    return 'JSON posted'

def api_response():
  from flask import jsonify
  if request.method == 'POST':
    return jsonify(**request.json)

app.run(host='0.0.0.0', port=5020)



"""
import cv2
vidcap = cv2.VideoCapture('C:/Users/Utente/Downloads/videoplayback (5).mp4')
success,image = vidcap.read()
ount = 0
while success:
  cv2.imwrite("C:/Users/Utente/Downloads/img/frame%d.jpg" % count, image)     # save frame as JPEG file
  success,image = vidcap.read()
  print('Read a new frame: ', success)
  count += 1
"""


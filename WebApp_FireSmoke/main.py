import os
# import magic
import urllib.request
from app import app
from flask import Flask, flash, request, redirect, render_template
from werkzeug.utils import secure_filename
import cv2
import shutil

ALLOWED_EXTENSIONS = set(['txt', 'pdf', 'png', 'jpg', 'jpeg', 'gif', 'avi'])


def allowed_file(filename):
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS


@app.route('/')
def upload_form():
    return render_template('upload.html')


@app.route('/', methods=['POST'])
def upload_file():
    if request.method == 'POST':
        # check if the post request has the file part
        if 'file' not in request.files:
            flash('No file part')
            return redirect(request.url)
        file = request.files['file']
        if file.filename == '':
            flash('No file selected for uploading')
            return redirect(request.url)
        if file and allowed_file(file.filename):
            filename = secure_filename(file.filename)
            filedirectory = (os.path.join(app.config['UPLOAD_FOLDER'], filename))
            file.save(os.path.join(app.config['UPLOAD_FOLDER'], filename))
            frame_from_video(filedirectory)
            flash('File successfully uploaded')
            return redirect('/')
        else:
            flash('Allowed file types are txt, pdf, png, jpg, jpeg, gif, avi')
            return redirect(request.url)
"""
def api_response():
  from flask import jsonify
  if request.method == 'POST':
    return jsonify(**request.json)
"""

def frame_from_video(filedirectory):
    """cancello le immagini presenti nella cartella"""
    folder = os.getcwd() + "/assets/inputs-predict/data/"
    for the_file in os.listdir(folder):
        file_path = os.path.join(folder, the_file)
        try:
            if os.path.isfile(file_path):
                os.unlink(file_path)
            # elif os.path.isdir(file_path): shutil.rmtree(file_path)
        except Exception as e:
            print(e)

    notepath = os.getcwd() + "/assets/inputs-predict/data/image_list.tsv"
    file1 = open(notepath, "w")
    vidcap = cv2.VideoCapture(filedirectory)
    vidcap.set(cv2.CAP_PROP_POS_FRAMES, 120)
    success, image = vidcap.read()
    count = 0
    while success:
        cv2.imwrite(os.getcwd() + "/assets/inputs-predict/data/frame_%d.jpg" % count, image)  # save frame as JPEG file
        cv2.imwrite(os.getcwd() + "/wwwroot//frame_%d.jpg" % count, image)
        success, image = vidcap.read()
        #vidcap.set(cv2.CAP_PROP_FRAME_COUNT, 5)
        print('Read a new frame%d: ' % count, success)
        file1.write("frame_%d.jpg\n" % count)
        count += 1
    file1.close()  # to change file access modes




if __name__ == "__main__":
    app.run(host='0.0.0.0', port=5020)
from PIL.ExifTags import TAGS


from PIL import Image

def get_exif(filename):
    image = Image.open(filename)
    image.verify()
    return image._getexif()

exif = get_exif('frame0.jpg')
print(exif)


def get_labeled_exif(exif):
    labeled = {}
    for (key, val) in exif.items():
        labeled[TAGS.get(key)] = val

    return labeled

exif = get_exif('frame0.jpg')
labeled = get_labeled_exif(exif)
print(labeled)
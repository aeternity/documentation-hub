FROM python:3-slim-stretch

# RUN apt-get update && \
#   apt-get install build-deps gcc python-dev musl-dev && \
#   apk add postgresql-dev

RUN apt-get update && apt-get install -y \
netbase \
build-essential

COPY . /data/

RUN pip install -r /data/requirements.txt

ENTRYPOINT [ "python", "/data/faucet.py"]
CMD [ "start"]
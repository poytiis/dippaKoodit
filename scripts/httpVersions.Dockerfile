FROM python:3.11-alpine
RUN apk add openssl-dev python3-dev bsd-compat-headers libffi-dev
COPY httpVersions.py .
ENTRYPOINT [ "python", "httpVersions.py" ]
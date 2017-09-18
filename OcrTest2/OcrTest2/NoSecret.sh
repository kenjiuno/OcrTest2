#!/bin/sh
sed -e "s/\"[0-9a-f]\+\"/\"SECRET\"/g" Secrets.cs  > Secrets_sample.cs
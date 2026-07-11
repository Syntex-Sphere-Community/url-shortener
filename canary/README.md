# url shortener

A simple CLI URL shortener written in Python. Generates a short code for any URL and stores the mapping in a local JSON file.

## Features

- Shorten a URL into a random short code
- Retrieve the original URL by its short code
- Show how many links are stored
- Colored terminal output with an interactive menu

## Install

```bash
cd canary
pip install -r requirements.txt
```

## Run

```bash
python main.py
```

Links are saved in `data/links.json`, which is created automatically on first use.

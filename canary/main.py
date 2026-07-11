import json
import os
import random
import string

import questionary
from art import text2art
from colorama import Fore, Style, init

init(autoreset=True)

DATA_FILE = "data/links.json"
CHARS = string.ascii_letters + string.digits


def print_gradient(text):
    colors = [
        Fore.LIGHTBLUE_EX,
        Fore.CYAN,
        Fore.LIGHTCYAN_EX,
        Fore.LIGHTGREEN_EX,
        Fore.GREEN,
    ]
    for i, line in enumerate(text.splitlines()):
        print(Style.BRIGHT + colors[i % len(colors)] + line)


def clear_console():
    os.system("cls" if os.name == "nt" else "clear")


def pause():
    input(Style.DIM + "\npress enter to return back to menu...")
    clear_console()


def load_links():
    try:
        with open(DATA_FILE, "r", encoding="utf-8") as file:
            return json.load(file)
    except FileNotFoundError:
        return {}


def save_links(links):
    os.makedirs(os.path.dirname(DATA_FILE), exist_ok=True)
    with open(DATA_FILE, "w", encoding="utf-8") as file:
        json.dump(links, file, ensure_ascii=False, indent=4)


def generate_short_code(length=6):
    return "".join(random.choice(CHARS) for _ in range(length))


def is_valid_url(url):
    return url.startswith(("http://", "https://"))


def shorten_url(long_url):
    links = load_links()
    for code, url in links.items():
        if url == long_url:
            return code

    short_code = generate_short_code()
    while short_code in links:
        short_code = generate_short_code()

    links[short_code] = long_url
    save_links(links)
    return short_code


def get_original_url(short_code):
    return load_links().get(short_code)


def main():
    banner = text2art("url   shortener", font="standard")
    while True:
        print_gradient(banner)
        print(Fore.LIGHTYELLOW_EX + "--- url shortener menu ---\n")

        try:
            choice = questionary.select(
                "choose option:",
                choices=[
                    "1. shorten url",
                    "2. get original url",
                    "3. show stats",
                    "4. exit",
                ],
            ).ask()
        except (EOFError, KeyboardInterrupt):
            choice = None

        if choice == "1. shorten url":
            long_url = input("enter url: ").strip()
            if not long_url:
                print(Fore.LIGHTRED_EX + "url cannot be empty")
            elif not is_valid_url(long_url):
                print(Fore.LIGHTRED_EX + "url must start with http:// or https://")
            else:
                print(Fore.LIGHTGREEN_EX + f"short code: {shorten_url(long_url)}")
            pause()

        elif choice == "2. get original url":
            short_code = input("enter short code: ").strip()
            if not short_code:
                print(Fore.LIGHTRED_EX + "short code cannot be empty")
            else:
                original_url = get_original_url(short_code)
                if original_url:
                    print(Fore.LIGHTBLUE_EX + f"original url: {original_url}")
                else:
                    print(Fore.LIGHTRED_EX + "short code not found")
            pause()

        elif choice == "3. show stats":
            print(Fore.LIGHTCYAN_EX + f"total links: {len(load_links())}")
            pause()

        else:
            print(Fore.CYAN + "bye!")
            clear_console()
            return


if __name__ == "__main__":
    main()

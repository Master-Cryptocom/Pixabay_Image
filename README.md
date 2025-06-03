# 🖼️ Image Search Pixabay

[![Release](https://img.shields.io/github/v/release/Master-Cryptocom/Pixabay_Image?label=release&logo=github)](https://github.com/Master-Cryptocom/Pixabay_Image/releases)
[![License](https://img.shields.io/github/license/Master-Cryptocom/Pixabay_Image?color=blue)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2064--bit-blue?logo=windows)]()
[![Language](https://img.shields.io/badge/made%20with-C%23-purple?logo=c-sharp)]()
[![Issues](https://img.shields.io/github/issues/Master-Cryptocom/Pixabay_Image?color=orange)](https://github.com/Master-Cryptocom/Pixabay_Image/issues)
[![Stars](https://img.shields.io/github/stars/Master-Cryptocom/Pixabay_Image?style=social)](https://github.com/Master-Cryptocom/Pixabay_Image/stargazers)
[![Telegram](https://img.shields.io/badge/Contact-Telegram-blue?logo=telegram)](https://t.me/master_cryptocom)

> **A fast and intuitive GUI application for searching and downloading royalty-free images from Pixabay.**  
> Explore over **5.5 million** free photos, illustrations, and vectors with advanced filtering options.

---

## 📸 Features

- 🔍 Powerful search interface with multilingual support  
- 🎨 Filters by **orientation**, **dominant color**, **image type**, **category**, **dimensions**, and **sort order**
- 🖼️ Live **preview panel** with detailed image info (author, tags, likes, views, size, downloads)
- 📁 Batch downloading with one-click selection  
- 🌐 Open images in browser or download directly to your device
- 🧰 API key setup with instant access to [Pixabay API Docs](https://pixabay.com/api/docs/)

---

## 🚀 Quick Start

1. **Download** the repository and extract the `Pixabay_Image_64` folder.  
2. **Run** `Pixabay.exe`.

   ![Startup Screenshot](screenshots/screenshot_3.png)

3. On first launch, enter your **Pixabay API key** (one-time setup).  
   If you don’t have one, click **"Get API Key"** – it will open the official [Pixabay API page](https://pixabay.com/api/docs/).  
   Log in with your Google account and copy the generated key.

   ![API Key Screenshot](screenshots/screenshot_4.png)

---

## 🧠 Filter Options

| Filter           | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| **Language**     | Query/tag language (e.g. `en`, `ru`, `de`, `fr`, etc.)                      |
| **Orientation**  | `all`, `horizontal`, `vertical`                                             |
| **Color**        | `all`, `grayscale`, `transparent`, `red`, `blue`, `white`, etc.            |
| **Image type**   | `all`, `photo`, `illustration`, `vector`                                   |
| **Category**     | `all`, `nature`, `technology`, `business`, `animals`, etc.                 |
| **Sort order**   | `popular`, `latest`                                                         |
| **Min Width/Height** | Exact dimensions in pixels (0–10000; `0` means any size)              |
| **Results per page** | Number of previews (1–200)                                             |

---

## 🖼️ Preview and Download

In the **image preview panel**, you can:
- View image details: author, size, tags, likes, views, downloads
- Open the image in your browser
- Download the image instantly
- Use **Select All + Download Selected** to fetch multiple images at once

Downloaded images are saved into the `/Image` folder with a **timestamp-based suffix** to avoid conflicts.

![Preview Screenshot](screenshots/screenshot_1.png)  
![Batch Screenshot](screenshots/screenshot_6.png)

---

## 🛠️ Tech Stack

- **Language:** C#
- **Platform:** Windows 64-bit
- **UI:** WinForms  
- **API:** [Pixabay REST API](https://pixabay.com/api/docs/)

---

## 📄 License

This project is released under the [MIT License](LICENSE).  
You are free to use, modify, and distribute this software.

---

## 📬 Contact & Support

If you have suggestions, questions, or ideas — feel free to contact me on [Telegram](https://t.me/master_cryptocom)  
Made with ❤️ by **Cryptocom**

---


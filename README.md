# ImageStitcher

![ImageSticher](https://github.com/user-attachments/assets/e8051d8b-f8ca-4d75-ab21-3e330e1d1a85)

## Description

ImageStitcher is a user-friendly desktop application for Windows built with C# and WPF on the .NET 8 platform. It allows you to easily combine two images, either side-by-side or one on top of the other, with several customization options.

## Features

- Dual Stitching Modes: Combine images horizontally or vertically.

- Flexible Image Alignment: When stitching images of different sizes, you can align the smaller image to the top/left, center, or bottom/right of the larger one.

- Drag-and-Drop: Quickly load your images by dragging them from a folder directly into the application's preview panes.

- File Dialog Support: Use the traditional "Select Image" buttons to browse for your files.

- Convenient UI Controls:

  - Swap Images: Instantly switch the positions of the two selected images with a single click.

  - Clear All: Reset the interface by removing both selected images.

- Custom Save Options: A "Save As" dialog lets you choose the output location, file name, and format (PNG, JPG, or BMP).

- Responsive Interface: The application remains responsive during the stitching process by performing heavy operations on a background thread, and it provides a "Working..." status to inform the user.

## How to Use

- Launch the Application: Open the ImageStitcher.exe file.

- Load Images: You can load images in two ways:

- Click the Select Image 1 and Select Image 2 buttons to open a file browser.

- Drag and drop your image files from your computer directly onto the corresponding left and right panes in the application window.

- Configure Options:

  - Stitch Direction: Choose either Horizontal or Vertical.

  - Image Alignment: Select how the images should be aligned relative to each other.

- Stitch and Save:

  - Click the Stitch & Save button.

  - A dialog will appear, allowing you to choose where to save the final image, what to name it, and which file format to use. Click Save.

- Confirmation: A message box will appear confirming that the image was saved successfully and showing the final file path.

## Requirements

-  Windows Operating System

- .NET 8.0 Desktop Runtime

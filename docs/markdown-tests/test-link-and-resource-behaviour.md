# Testing link and resource behaviour in the Awesomium preview control

See `DocumentView.xaml.cs`, specifically:

- `WebControlLinkClicked()`, and
- `WebControlResourceRequest()`


## Working scenarios

### Resource request for remote resources
This should show the Daring Fireball logo, deep-linked from the site itself:

![DF](http://daringfireball.net/graphics/logos/)

### Resource request for local resources
This should show the MarkPad icon, stored alongside this document:

<img alt="MarkPad icon" src="icon.png"/>

### Links for remote resources
This link to the [Code52](http://code52.org) site should open in a new browser window or tab.

### Links for local resources that do not exist
- Clicking this link to a non-existent url [should do nothing](abcd)
- Clicking this links to an empty url [should flash an error but immediately re-render the page]()
- Right-clicking the preview pane and selecting *Refresh*, or left-clicking the preview and pressing `F5` should also flash an error but immediately re-render the page


## Broken or unreliable scenarios
### Links for local resources that exist
This is a [link to the local MarkPad icon](icon.png). Usually the file will open using `Process.Start()` (preferred behaviour), but sometimes it will open in the preview. This behaviour is erratic but appears to be an upstream issue with the Awesomium control.

### Alt text for images where the image is not found
Alt text is not displayed for images. The following `img` tags should render as the text "This image is missing" but instead an empty box is displayed. This appears to be an upstream issue with the Awesomium control.

- ![This image is missing](missing.jpg)
- <img alt="This image is missing" src="missing.jpg"/>


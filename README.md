# P&O Entrepreneurship - Team A - Virtual Company Assistant (code)

## Set-up
###### Set up the Virtual Environment using PyCharm
1. Install Anaconda, if you haven't done that already.
2. Open PyCharm.
3. Create a conda virtual environment following the [PyCharm instructions](https://www.jetbrains.com/help/pycharm/conda-support-creating-conda-virtual-environment.html).
4. If this doesn't work, make sure the path to the conda interpreter is valid.

:ok: When opening the Terminal in PyCharm, you should see the path to your virtual environment between brackets in front of the current path:
```
(D:\Documents\P-O-Entrepreneurship-Team-A-code\venv) D:\Documents\P-O-Entrepreneurship-Team-A-code>
```

###### When you need a library that hasn't been installed yet
1. Open PyCharm
2. Install package/library following the [instructions from the PyCharm documentation](https://www.jetbrains.com/help/pycharm/installing-uninstalling-and-upgrading-packages.html).

:warning: Make sure the "Use Conda Package Manager" toggle is enabled
4. Add a line to the `requirements.txt` file in the root of the repo: "[package-name]==[installed-version]", e.g. "pdoc3==0.7.2"

:warning: Make sure to commit and push this change

###### When someone else has added libraries to the `requirements.txt` file that you haven't installed yet
1. PyCharm will show a bar "Package requirement	... not satisfied".
2. Click the install link to install missing packages.

###### Testing whether set-up is working
- For now, I don't know what's the best way to test this, but I'd recommend to follow above steps and afterwards open the Terminal (*not the Python Console*) in PyCharm.
Execute
```
pdoc
```
This should return something like
```
usage: pdoc-script.py [-h] [--version] [-c OPTION=VALUE] [--filter STRING]
                      [-f] [--html | --pdf] [-o DIR] [--template-dir DIR]
                      [--close-stdin] [--http HOST:PORT]
                      MODULE [MODULE ...]
pdoc-script.py: error: the following arguments are required: MODULE
```

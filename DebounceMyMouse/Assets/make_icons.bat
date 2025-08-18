@echo off
set SRC=app.svg
for %%S in (16 20 24 32 40 48 64 128 256) do (
  inkscape "%SRC%" -o "icon_%%S.png" -w %%S -h %%S
)
echo Done.
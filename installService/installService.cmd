@echo off

nssm install DrcomDontnetNssm "%~dp0\DrComDotnet.exe"
nssm set DrcomDontnetNssm AppStdout "%~dp0\log.txt"
sc failure DrcomDontnetNssm reset= 3 actions= restart
sc start DrcomDontnetNssm

pause
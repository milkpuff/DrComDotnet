@echo off
:: DrcomDotnet nssm service installer
:: version: 1.0.0
:: Copyright 2020 Leviolet.
:: This file is part of DrComDotnet.
:: DrComDotnet is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, version 3.
:: DrComDotnet is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Affero General Public License for more details.
:: You should have received a copy of the GNU Affero General Public License along with DrComDotnet.  If not, see <https://www.gnu.org/licenses/>.


nssm install DrcomDontnetNssm "%~dp0\DrComDotnet.exe"
nssm set DrcomDontnetNssm AppStdout "%~dp0\log.txt"
sc failure DrcomDontnetNssm reset= 3 actions= restart
sc start DrcomDontnetNssm

pause
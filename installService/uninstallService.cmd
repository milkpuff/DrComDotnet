@echo off
:: DrcomDotnet nssm service uninstaller
:: version: 1.0.0
:: Copyright 2020 Leviolet.
:: This file is part of DrComDotnet.
:: DrComDotnet is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, version 3.
:: DrComDotnet is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Affero General Public License for more details.
:: You should have received a copy of the GNU Affero General Public License along with DrComDotnet.  If not, see <https://www.gnu.org/licenses/>.


sc   stop   DrcomDotnetService
nssm remove DrcomDotnetService

pause
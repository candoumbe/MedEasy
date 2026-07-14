#!/usr/bin/env bash
set -euo pipefail

# This script is run during the "Create" phase of the dev container lifecycle, which occurs after the container is built 
# but before it is started. It is used to perform any setup tasks that need to be done once per container creation,
# such as installing additional tools or configuring the environment.

# Some shells do not load the Node feature profile hooks in non-interactive scripts.
# Ensure npm is available for Aspire JavaScript resources and build tasks.
if ! command -v npm >/dev/null 2>&1; then
	echo "[create] npm not found in PATH, installing nodejs/npm from apt"
	sudo apt-get update
	sudo apt-get install -y nodejs npm
	sudo apt-get clean
	sudo rm -rf /var/lib/apt/lists/*
fi

echo "[create] Installing Aspire CLI"
curl -sSL https://aspire.dev/install.sh | bash
echo "[create] Aspire CLI installation complete"

# install rg command for searching code
if ! command -v rg >/dev/null 2>&1; then
    echo "[create] rg (ripgrep) not found in PATH, installing from apt"
    sudo apt-get update
    sudo apt-get install -y ripgrep
    sudo apt-get clean
    sudo rm -rf /var/lib/apt/lists/*
fi
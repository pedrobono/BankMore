#!/bin/bash

# Script para aumentar o limite de inotify instances no Linux
# Necessário para executar testes de integração que criam múltiplas instâncias do WebApplicationFactory

echo "Aumentando limite de inotify instances..."

# Temporário (até reiniciar)
echo 512 | sudo tee /proc/sys/fs/inotify/max_user_instances
echo 524288 | sudo tee /proc/sys/fs/inotify/max_user_watches

# Permanente (sobrevive a reinicializações)
echo "fs.inotify.max_user_instances=512" | sudo tee -a /etc/sysctl.conf
echo "fs.inotify.max_user_watches=524288" | sudo tee -a /etc/sysctl.conf

# Aplicar mudanças
sudo sysctl -p

echo "✅ Limites aumentados com sucesso!"
echo "Valores atuais:"
cat /proc/sys/fs/inotify/max_user_instances
cat /proc/sys/fs/inotify/max_user_watches

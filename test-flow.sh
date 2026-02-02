#!/bin/bash

# Configurações
CPF1="92955412007"
CPF2="99414063080"
SENHA="senha123"

echo "=========================================="
echo "1. CRIAR CONTA 1 - Pedro Henrique Bono"
echo "=========================================="
CONTA1=$(curl -s -X POST http://localhost:8081/api/Conta \
  -H "Content-Type: application/json" \
  -d "{
    \"cpf\": \"$CPF1\",
    \"senha\": \"$SENHA\",
    \"nomeTitular\": \"Pedro Henrique Bono\"
  }")

echo "$CONTA1" | jq '.'
CONTA1_NUMERO=$(echo "$CONTA1" | jq -r '.numeroConta')
echo ""
echo "✅ Conta 1 criada: Número=$CONTA1_NUMERO"
echo ""

sleep 2

echo "=========================================="
echo "2. CRIAR CONTA 2 - Outra Conta"
echo "=========================================="
CONTA2=$(curl -s -X POST http://localhost:8081/api/Conta \
  -H "Content-Type: application/json" \
  -d "{
    \"cpf\": \"$CPF2\",
    \"senha\": \"$SENHA\",
    \"nomeTitular\": \"outra conta\"
  }")

echo "$CONTA2" | jq '.'
CONTA2_NUMERO=$(echo "$CONTA2" | jq -r '.numeroConta')
echo ""
echo "✅ Conta 2 criada: Número=$CONTA2_NUMERO"
echo ""

sleep 2

echo "=========================================="
echo "3. LOGIN CONTA 1"
echo "=========================================="
LOGIN1=$(curl -s -X POST http://localhost:8081/api/Auth/login \
  -H "Content-Type: application/json" \
  -d "{
    \"cpfOrNumeroConta\": \"$CPF1\",
    \"senha\": \"$SENHA\"
  }")

echo "$LOGIN1" | jq '.'
TOKEN1=$(echo "$LOGIN1" | jq -r '.token')
echo ""
echo "✅ Token Conta 1: $TOKEN1"
echo ""

sleep 2

echo "=========================================="
echo "4. LOGIN CONTA 2"
echo "=========================================="
LOGIN2=$(curl -s -X POST http://localhost:8081/api/Auth/login \
  -H "Content-Type: application/json" \
  -d "{
    \"cpfOrNumeroConta\": \"$CPF2\",
    \"senha\": \"$SENHA\"
  }")

echo "$LOGIN2" | jq '.'
TOKEN2=$(echo "$LOGIN2" | jq -r '.token')
echo ""
echo "✅ Token Conta 2: $TOKEN2"
echo ""

sleep 2

echo "=========================================="
echo "5. CREDITAR R$ 5000 NA CONTA 1"
echo "=========================================="
CREDITO=$(curl -s -X POST http://localhost:8081/api/Movimento \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN1" \
  -d "{
    \"requestId\": \"$(uuidgen)\",
    \"valor\": 5000,
    \"tipo\": \"C\"
  }")

echo "$CREDITO" | jq '.'
echo ""
echo "✅ Crédito realizado"
echo ""

sleep 2

echo "=========================================="
echo "6. CONSULTAR SALDO CONTA 1"
echo "=========================================="
SALDO1=$(curl -s -X GET "http://localhost:8081/api/Saldo" \
  -H "Authorization: Bearer $TOKEN1")

echo "$SALDO1" | jq '.'
echo ""

sleep 2

echo "=========================================="
echo "7. TRANSFERIR R$ 50 DA CONTA 1 PARA CONTA 2"
echo "=========================================="
TRANSFERENCIA=$(curl -s -X POST http://localhost:8082/Transferencia \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN1" \
  -d "{
    \"requestId\": \"$(uuidgen)\",
    \"numeroContaDestino\": \"$CONTA2_NUMERO\",
    \"valor\": 50
  }")

echo "$TRANSFERENCIA" | jq '.'
echo ""
echo "✅ Transferência realizada"
echo ""

sleep 5

echo "=========================================="
echo "8. CONSULTAR SALDO FINAL CONTA 1"
echo "=========================================="
SALDO1_FINAL=$(curl -s -X GET "http://localhost:8081/api/Saldo" \
  -H "Authorization: Bearer $TOKEN1")

echo "$SALDO1_FINAL" | jq '.'
echo ""
echo "💰 Saldo esperado: R$ 4948,00 (5000 - 50 - 2 tarifa)"
echo ""

sleep 2

echo "=========================================="
echo "9. CONSULTAR SALDO FINAL CONTA 2"
echo "=========================================="
SALDO2_FINAL=$(curl -s -X GET "http://localhost:8081/api/Saldo" \
  -H "Authorization: Bearer $TOKEN2")

echo "$SALDO2_FINAL" | jq '.'
echo ""
echo "💰 Saldo esperado: R$ 50,00"
echo ""

echo "=========================================="
echo "✅ TESTE COMPLETO FINALIZADO!"
echo "=========================================="
echo ""
echo "📝 RESUMO:"
echo "  Conta 1: $CONTA1_NUMERO"
echo "  Conta 2: $CONTA2_NUMERO"
echo "  Transferência: R$ 50,00"
echo "  Tarifa: R$ 2,00"
echo "=========================================="

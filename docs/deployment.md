# Инструкции по развёртыванию

---

## Предварительные требования

### Серверы

| Компонент | Минимум | Рекомендация |
|-----------|---------|--------------|
| API Server | 4 CPU, 8 GB RAM | 8 CPU, 16 GB RAM |
| PostgreSQL | 2 CPU, 4 GB RAM | 4 CPU, 8 GB RAM |
| Redis | 1 CPU, 2 GB RAM | 2 CPU, 4 GB RAM |
| RabbitMQ | 2 CPU, 4 GB RAM | 4 CPU, 8 GB RAM |
| Storage | 100 GB SSD | 500 GB SSD |

### Программное обеспечение

- Docker 20.10+
- Docker Compose 2.0+
- PostgreSQL 16+
- Redis 7.0+
- RabbitMQ 3.12+
- Nginx (reverse proxy)

---

## Развёртывание через Docker Compose

### 1. Клонирование репозитория

```bash
git clone https://github.com/samorodinkatech/fiducia.git
cd fiducia
```

### 2. Настройка переменных окружения

```bash
cp .env.example .env
```

Отредактируйте `.env`:

```env
# Database
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
POSTGRES_DB=fiducia
POSTGRES_USER=fiducia
POSTGRES_PASSWORD=your-secure-password

# Redis
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_PASSWORD=your-redis-password

# RabbitMQ
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=fiducia
RABBITMQ_PASSWORD=your-rabbitmq-password

# Application
ASPNETCORE_ENVIRONMENT=Production
API_KEY=your-api-key
JWT_SECRET=your-jwt-secret

# Storage
MINIO_ENDPOINT=minio:9000
MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin
```

### 3. Сборка и запуск

```bash
# Сборка образов
docker-compose build

# Запуск всех сервисов
docker-compose up -d

# Проверка статуса
docker-compose ps

# Просмотр логов
docker-compose logs -f api
```

### 4. Инициализация базы данных

```bash
# Применение миграций
docker-compose exec api dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Api
```

### 5. Проверка работоспособности

```bash
# Health check
curl http://localhost:5001/health

# Expected response
{"status": "healthy"}
```

---

## Развёртывание на Kubernetes

### 1. Подготовка

```bash
# Создание namespace
kubectl create namespace fiducia

# Применение секретов
kubectl apply -f k8s/secrets.yaml -n fiducia
```

### 2. Развёртывание

```bash
# PostgreSQL
kubectl apply -f k8s/postgres/ -n fiducia

# Redis
kubectl apply -f k8s/redis/ -n fiducia

# RabbitMQ
kubectl apply -f k8s/rabbitmq/ -n fiducia

# API
kubectl apply -f k8s/api/ -n fiducia
```

### 3. Проверка

```bash
# Статус подов
kubectl get pods -n fiducia

# Логи
kubectl logs -f deployment/api -n fiducia

# Port-forward для доступа
kubectl port-forward svc/api-service 5000:80 -n fiducia
```

---

## Настройка Nginx (Reverse Proxy)

### Конфигурация

```nginx
upstream fiducia_api {
    server api-1:5000;
    server api-2:5000;
}

server {
    listen 443 ssl http2;
    server_name board.samorodinkatech.ru;

    ssl_certificate /etc/ssl/certs/board.crt;
    ssl_certificate_key /etc/ssl/private/board.key;

    # Security headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Content-Type-Options nosniff always;
    add_header X-Frame-Options DENY always;

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api:10m rate=100r/m;

    location / {
        limit_req zone=api burst=20 nodelay;

        proxy_pass http://fiducia_api;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /health {
        proxy_pass http://fiducia_api;
    }
}
```

---

## Настройка SSL/TLS

### Let's Encrypt (рекомендуется)

```bash
# Установка certbot
sudo apt install certbot python3-certbot-nginx

# Получение сертификата
sudo certbot --nginx -d board.samorodinkatech.ru

# Автоматическое обновление
sudo crontab -e
# Добавить: 0 12 * * * /usr/bin/certbot renew --quiet
```

### Самоподписанные сертификаты (для тестов)

```bash
# Создание CA
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ca.key -out ca.crt \
  -subj "/CN=Fiducia CA"

# Создание серверного сертификата
openssl req -nodes -newkey rsa:2048 \
  -keyout server.key -out server.csr \
  -subj "/CN=board.samorodinkatech.ru"

openssl x509 -req -days 365 -in server.csr \
  -CA ca.crt -CAkey ca.key -CAcreateserial \
  -out server.crt
```

---

## Настройка мониторинга

### Prometheus

```yaml
# prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'fiducia-api'
    static_configs:
      - targets: ['api:5000']
    metrics_path: '/metrics'
```

### Grafana

```bash
# Дашборды по умолчанию
# Import из: https://grafana.com/grafana/dashboards/
# ID: 1860 (Node Exporter)
# ID: 6693 (Docker)
```

### Alert Rules

```yaml
# alerts.yml
groups:
  - name: fiducia
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.05
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High error rate detected"

      - alert: HighLatency
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High latency detected"
```

---

## Backup и Restore

### PostgreSQL

```bash
# Бэкап
docker-compose exec postgres pg_dump -U fiducia fiducia > backup_$(date +%Y%m%d).sql

# Восстановление
cat backup_20260115.sql | docker-compose exec -T postgres psql -U fiducia fiducia

# Автоматический бэкап (crontab)
0 2 * * * docker-compose exec postgres pg_dump -U fiducia fiducia | gzip > /backups/fiducia_$(date +\%Y\%m\%d).sql.gz
```

### Redis

```bash
# Бэкап
docker-compose exec redis redis-cli BGSAVE

# Восстановление
docker-compose cp redis:/data/dump.rdb ./
docker-compose restart redis
```

### MinIO

```bash
# Бэкап через mc
mc mirror myminio/fiducia /backups/minio/
```

---

## Логирование

### Структурированные логи

```json
{
  "timestamp": "2026-01-15T10:30:00Z",
  "level": "Information",
  "message": "Meeting created",
  "properties": {
    "meetingId": 789,
    "createdBy": 456,
    "duration_ms": 150
  }
}
```

### Aggregation (ELK Stack)

```yaml
# docker-compose.elk.yml
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.12.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false

  logstash:
    image: docker.elastic.co/logstash/logstash:8.12.0
    volumes:
      - ./logstash.conf:/usr/share/logstash/pipeline/logstash.conf

  kibana:
    image: docker.elastic.co/kibana/kibana:8.12.0
    ports:
      - "5601:5601"
```

---

## Откат версий

### Docker Compose

```bash
# Остановка текущей версии
docker-compose down

# Запуск предыдущей версии
docker-compose -f docker-compose.old.yml up -d
```

### Kubernetes

```bash
# Просмотр истории
kubectl rollout history deployment/api -n fiducia

# Откат
kubectl rollout undo deployment/api -n fiducia

# Откат к конкретной версии
kubectl rollout undo deployment/api --to-revision=3 -n fiducia
```

---

## Чеклист перед релизом

- [ ] Все тесты пройдены
- [ ] Миграции протестированы
- [ ] Бэкап сделан
- [ ] SSL сертификаты обновлены
- [ ] Логи проверены
- [ ] Мониторинг настроен
- [ ] Alert Rules протестированы
- [ ] Rollback план готов
- [ ] Команда уведомлена

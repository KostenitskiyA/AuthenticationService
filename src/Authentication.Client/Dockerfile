FROM node:20 AS builder
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build:staging

FROM node:20 AS runner
WORKDIR /app

# Только необходимые файлы
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/package*.json ./

RUN npm install --omit=dev

EXPOSE 4173
CMD ["npm", "run", "deploy:staging"]

import pika
import json

connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq'))
channel = connection.channel()
channel.queue_declare(queue='rssi_queue')

def callback(ch, method, properties, body):
    data = json.loads(body)
    print(f"Processing RSSI: {data}")
    result = {"attendance": "present" if data.get("rssi", -100) > -70 else "absent",
              "timestamp": data.get("timestamp")}
    channel.basic_publish(exchange='', routing_key='result_queue', body=json.dumps(result))
    ch.basic_ack(delivery_tag=method.delivery_tag)

channel.basic_consume(queue='rssi_queue', on_message_callback=callback)
print("Waiting for RSSI messages...")
channel.start_consuming()

# Ecommerce

A micro-service ecommerce based platform using data from instacart. 

The centre of this application is Kafka as the message broker, which is faciliating delivery of messages to other parts of the platform. 

## Running to application
Use `docker-compose up -d` to configure all the external dependancies. 

## Overview
### Data Seeder
Responsible for reading input data and raising commands for other services to create initial data. 

### Product Service
Create and manages product information. Uses MySQL to store data and is subscribed to events to create a product. Makes use of Mediator library to help implement CQRS pattern. 

## Future Developments
1. Product service
    1. Methods to retrieve product info
    1. gRPC for inter-service communications
1. Order service
    1. Event sourcing for managing state of the order
1. Stock service
1. Create react/node.js front end
    1. Elasticsearch for search page
    1. Investigate graphql for product info pages
1. Recommendation Service
    1. ML based to suggest products other users have bought
    1. Suggest to buy products based on frequency between purchases and since last purchase
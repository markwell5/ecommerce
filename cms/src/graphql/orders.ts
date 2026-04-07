import { gql } from '@apollo/client';

export const GET_ORDERS = gql`
  query GetOrders($page: Int!, $pageSize: Int!, $status: String) {
    orders(page: $page, pageSize: $pageSize, status: $status) {
      items {
        orderId
        customerId
        status
        totalAmount
        items {
          productId
          quantity
          unitPrice
        }
        customer {
          firstName
          lastName
          email
        }
        createdAt
        updatedAt
      }
      totalCount
      page
      pageSize
    }
  }
`;

export const CANCEL_ORDER = gql`
  mutation CancelOrder($orderId: String!) {
    cancelOrder(orderId: $orderId)
  }
`;

export const SHIP_ORDER = gql`
  mutation ShipOrder($orderId: String!) {
    shipOrder(orderId: $orderId)
  }
`;

export const DELIVER_ORDER = gql`
  mutation DeliverOrder($orderId: String!) {
    deliverOrder(orderId: $orderId)
  }
`;

export const RETURN_ORDER = gql`
  mutation ReturnOrder($orderId: String!) {
    returnOrder(orderId: $orderId)
  }
`;

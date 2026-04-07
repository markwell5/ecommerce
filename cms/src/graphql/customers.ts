import { gql } from '@apollo/client';

export const GET_USERS = gql`
  query GetUsers($page: Int!, $pageSize: Int!, $search: String) {
    users(page: $page, pageSize: $pageSize, search: $search) {
      items {
        id
        email
        firstName
        lastName
        phone
        role
        createdAt
      }
      totalCount
      page
      pageSize
    }
  }
`;

export const GET_USER = gql`
  query GetUser($userId: String!) {
    user(userId: $userId) {
      id
      email
      firstName
      lastName
      phone
      role
      createdAt
    }
  }
`;

export const GET_USER_ADDRESSES = gql`
  query GetUserAddresses($userId: String!) {
    userAddresses(userId: $userId) {
      id
      line1
      line2
      city
      county
      postCode
      country
      isDefault
    }
  }
`;

export const GET_ORDERS_BY_CUSTOMER = gql`
  query GetOrdersByCustomer($customerId: String!) {
    ordersByCustomer(customerId: $customerId) {
      orderId
      status
      totalAmount
      createdAt
    }
  }
`;

export const GET_PAYMENTS_BY_CUSTOMER = gql`
  query GetPaymentsByCustomer($customerId: String!) {
    paymentsByCustomer(customerId: $customerId) {
      id
      orderId
      amount
      currency
      status
      createdAt
    }
  }
`;

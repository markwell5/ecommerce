import { gql } from '@apollo/client';

export const GET_DASHBOARD_ORDERS = gql`
  query GetDashboardOrders($page: Int!, $pageSize: Int!) {
    orders(page: $page, pageSize: $pageSize) {
      items {
        orderId
        status
        totalAmount
        createdAt
      }
      totalCount
    }
  }
`;

export const GET_DASHBOARD_PRODUCTS = gql`
  query GetDashboardProducts($page: Int!, $pageSize: Int!) {
    products(page: $page, pageSize: $pageSize) {
      items {
        id
        name
        category
        price
        stockLevel {
          availableQuantity
        }
      }
      totalCount
    }
  }
`;

import '../styles/globals.css'
import { Auth0Provider } from "@auth0/auth0-react";

function MyApp({ Component, pageProps }) {
  return (<Auth0Provider
    domain="dev-di8ylomu.eu.auth0.com"
    clientId="5vm0e7e6s2hZdrl63n6PHg1Fc0nCTah8"
    redirectUri="http://localhost:3000/"
  >
    <Component {...pageProps} />
  </Auth0Provider>);
}

export default MyApp

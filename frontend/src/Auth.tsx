import React from 'react';
import createAuth0Client, { User } from '@auth0/auth0-spa-js';
import Auth0Client from '@auth0/auth0-spa-js/dist/typings/Auth0Client';
import { authSettings } from './AppSettings';

interface Auth0User {
  name: string;
  email: string;
}

interface IAuth0Context {
  isAuthenticated: boolean;
  //user?: Auth0User;
  user?: User;
  signIn: () => void;
  signOut: () => void;
  loading: boolean;
}

export const Auth0Conext = React.createContext<IAuth0Context>({
  isAuthenticated: false,
  signIn: () => {},
  signOut: () => {},
  loading: true,
});

export const useAuth = () => React.useContext(Auth0Conext);

export const AuthProvider: React.FC = ({ children }) => {
  //whether the user is authenticated
  const [isAuthenticated, setIsAuthenticated] = React.useState<boolean>(false);
  //the user's profile info
  //const [user, setUser] = React.useState<Auth0User | undefined>(undefined);
  const [user, setUser] = React.useState<User | undefined>(undefined);
  //a client object from Auth0
  const [auth0Client, setAuth0Client] = React.useState<Auth0Client>();
  //whether the context is loading
  const [loading, setLoading] = React.useState<boolean>(true);

  const getAuth0ClientFromState = () => {
    if (auth0Client === undefined) {
      throw new Error('Auth0 Client not set');
    }
    return auth0Client;
  };

  React.useEffect(() => {
    const initAuth0 = async () => {
      setLoading(true);
      //create the Auth0 client instance
      const auth0FromHook = await createAuth0Client(authSettings);
      setAuth0Client(auth0FromHook);

      if (
        window.location.pathname === '/signin-callback' &&
        window.location.search.indexOf('code=') > -1
      ) {
        await auth0FromHook.handleRedirectCallback();
        window.location.replace(window.location.origin);
      }

      //determine whether the user is authenticated
      const isAuthenticatedFromHook = await auth0FromHook.isAuthenticated();
      if (isAuthenticatedFromHook) {
        const user = await auth0FromHook.getUser();
        // const currAuth0User: Auth0User = {
        //   name: user?.name ?? '',
        //   email: user?.email ?? '',
        // };
        console.log('user is authenticated!');
        console.log(user);
        setUser(user);
        //setUser(currAuth0User);
        //console.log(currAuth0User);
      }
      setIsAuthenticated(isAuthenticatedFromHook);
      setLoading(false);
    };
    initAuth0();
  }, []);

  return (
    <Auth0Conext.Provider
      value={{
        isAuthenticated,
        user,
        signIn: () => getAuth0ClientFromState().loginWithRedirect(),
        signOut: () =>
          getAuth0ClientFromState().logout({
            client_id: authSettings.client_id,
            returnTo: window.location.origin + '/signout-callback',
          }),
        loading,
      }}
    >
      {children}
    </Auth0Conext.Provider>
  );
};

export const getAccessToken = async () => {
  const auth0FromHook = await createAuth0Client(authSettings);
  const accessToken = await auth0FromHook.getTokenSilently();
  return accessToken;
};

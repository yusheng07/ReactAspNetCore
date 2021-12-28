import React from 'react';
import { Page } from './Page';
import { StatusText } from './Styles';
import { useAuth } from './Auth';

type SignoutAction = 'signout' | 'signout-callback';

interface Props {
  action: SignoutAction;
}

export const SignOutPage = ({ action }: Props) => {
  let message = 'Signing out ...';

  const { signOut } = useAuth();

  switch (action) {
    case 'signout':
      signOut();
      break;
    case 'signout-callback':
      message = 'Successfully signed out!';
      break;
  }

  return (
    <Page title="Sign Out">
      <StatusText>{message}</StatusText>
    </Page>
  );
};

export const server = 'https://localhost:44381';

export const webAPIUrl = `${server}/api`;

export const authSettings = {
  domain: 'yusheng07.us.auth0.com',
  client_id: 'lktd5ZEJm8gh5sD4TyYRv1uBskGDNQQN',
  redirect_uri: window.location.origin + '/signin-callback',
  scope: 'openid profile QandAAPI email',
  audience: 'https://qanda',
};

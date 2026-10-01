/** The ready-made message a manager sends to a new guide (wording from docs/API-V11-WEB.md, "Guides"). */
export function guideShareMessage(email: string, temporaryPassword: string): string {
  return (
    `Your TripCraft guide login: ${email} / temporary password ${temporaryPassword}. ` +
    'Open the TripCraft app and sign in; you will be asked to choose a new password.'
  );
}

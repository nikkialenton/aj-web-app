export interface GuestLookup {
  firstName: string;
  lastName: string;
  allowedPlusOne: boolean;
  hasRsvped: boolean;
  existingRsvp?: RsvpView;
}

export interface RsvpView {
  isAttending: boolean;
  plusOneAttending?: boolean;
  plusOneName: string;
  message: string;
  submittedAt: string;
}

export interface RsvpCreate {
  isAttending: boolean;
  plusOneAttending?: boolean;
  plusOneName: string;
  message: string;
}

export interface GuestAdmin {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  token: string;
  allowedPlusOne: boolean;
  groupName: string;
  hasRsvped: boolean;
  rsvp?: RsvpView;
}

export interface GuestCreate {
  firstName: string;
  lastName: string;
  email: string;
  allowedPlusOne: boolean;
  groupName: string;
}

export interface AdminStats {
  totalGuests: number;
  rsvpedCount: number;
  pendingCount: number;
  attending: number;
  declined: number;
  totalAttending: number;
}

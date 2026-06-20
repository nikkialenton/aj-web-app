export interface GuestLookup {
  firstName: string;
  lastName: string;
  allowedGuests: number;
  hasRsvped: boolean;
  existingRsvp?: RsvpView;
}

export interface RsvpView {
  isAttending: boolean;
  additionalGuests: string[];
  message: string;
  submittedAt: string;
}

export interface RsvpCreate {
  isAttending: boolean;
  additionalGuests: string[];
  message: string;
}

export interface GuestAdmin {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  token: string;
  allowedGuests: number;
  groupName: string;
  hasRsvped: boolean;
  rsvp?: RsvpView;
}

export interface GuestCreate {
  firstName: string;
  lastName: string;
  email: string;
  allowedGuests: number;
  groupName: string;
}

export interface GuestUpdate {
  firstName: string;
  lastName: string;
  groupName: string;
  allowedGuests: number;
}

export interface AdminStats {
  totalGuests: number;
  rsvpedCount: number;
  pendingCount: number;
  attending: number;
  declined: number;
  totalAttending: number;
}

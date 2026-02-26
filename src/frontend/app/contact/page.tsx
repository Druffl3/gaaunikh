import { SiteShell } from "../../components/site-shell";
import { routeContent } from "../../components/route-content";

export default function ContactPage() {
  return (
    <SiteShell
      heading={routeContent.contact.heading}
      description={routeContent.contact.description}
      actionText={routeContent.contact.actionText}
    />
  );
}


import { SiteShell } from "../../components/site-shell";
import { routeContent } from "../../components/route-content";

export default function TrackOrderPage() {
  return (
    <SiteShell
      heading={routeContent.trackOrder.heading}
      description={routeContent.trackOrder.description}
      actionText={routeContent.trackOrder.actionText}
    />
  );
}


# Scientific challenge discovery (v0.4.0)

The XR research window now has a bounded public-notice discovery adapter. On startup after valid headset tracking, it requests three fixed English terms (prize competition, challenge, hackathon) from the FederalRegister.gov public API. No API key or payment is required. The service is operated by the U.S. government; its web edition is an informational rendition, and the official publication should be consulted for legal rules. The endpoint schema was reviewed, but live API response and Unity runtime verification remain pending.

Each query requests up to 40 recent records, with a 15-second timeout, no redirects, a 2 MiB response limit and a 30-second manual refresh cooldown. Only Notice records with challenge-related titles, valid dates, identifiers and HTTPS links to the expected host are accepted. Duplicate document identifiers are removed. Results are public announcements, not a verified list of open competitions. Some notices are unrelated, expired, cancellations or corrections. The user must check official rules, current dates, eligibility and the original announcement. The publication date is NOT a submission deadline. No application, registration or submission is automated.

Search keywords are matched locally against the downloaded original titles (all words required, case and accent insensitive). Keywords, conversations, notes and patient information are never sent to the endpoint. The first page per fixed query is a limited sample, not comprehensive global coverage. No data is fabricated. Empty results, source failure and limited results are distinct states. Refresh replaces the previous in-memory results; no disk cache is included. Source titles are retained in the original language, with localized UI labels; automatic translation and multilingual synonym expansion are pending. Search in the source language for best results.

The official portals control opens a separate view of three verified portal links (NIH, Sage/DREAM and NASA Space Apps). These are navigation links, NOT automatically retrieved challenges. Do not scrape portals or bypass access restrictions. Review documented interfaces and terms before adding more adapters. Opening a link launches the platform browser via Application.OpenURL, not an embedded browser. No public datasets are downloaded yet.

## Controls and tests

Search keywords opens the supported keyboard; Refresh updates the public notices; Next cycles the current results or portal; Open source opens its validated official link; Official portals toggles navigation; All topics clears the local filter. The window retains resize, grip movement and saved layout behavior. No new paid API or media is used. Public network access is required for discovery; AI remains local only.

Run VITQRA > Run Discovery Checks in Unity Editor for synthetic parsing and URL validation checks. Fixtures are not real notices and are never inserted into the application results. Pending: compile in Unity, verify actual endpoint response, Android Internet permission and keyboard, test timeout and interrupted requests on Quest, inspect current eligibility/deadlines, translate original content using reviewed local models, add authorized biomedical and hackathon data sources, and build an embedded browser.

References:
- https://www.federalregister.gov/developers/documentation/api/v1
- https://www.nih.gov/challenges
- https://challenges.synapse.org/OpenChallenges
- https://www.spaceappschallenge.org/

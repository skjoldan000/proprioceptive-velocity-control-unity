library(tidyverse)
library(ggforce)

dataPath <- "../../testData/run1/1/S001/"


trials <- read_csv(paste0(dataPath, "trial_results.csv"))

trials <- trials %>% 
  mutate(dfRow = row_number(),
         trialID = paste(ppid, experiment, session_num, block_num, trial_num, sep = "_"))

tracking_files <- list.files(path = paste0(dataPath, "trackers/"))
tracking <- read_csv(paste0(dataPath, "trackers/", tracking_files))
tracking <- tracking %>% 
  select(-c(truepos_y, vispos_y)) %>% 
  filter(!trialProgress %in% c("calibrationStarted", "trialSetup", "trialInput"))
  
tracking %>% pull(trialProgress) %>% unique()

tracking <- tracking %>%
  pivot_longer(
    cols = c(truepos_x, truepos_z, vispos_x, vispos_z),  # The columns to pivot
    names_to = c("condition", ".value"),                 # Split into 'condition' and the value columns ('x' and 'z')
    names_sep = "_"                                      # Use '_' to separate 'truepos'/'vispos' from 'x'/'z'
  ) %>% 
  mutate(index= row_number())

tracking <- tracking %>% 
  left_join(trials, by = "trialID")

target <- geom_circle(aes(x0 = 0, y0 = 0.4, r = 0.025), color = NA, fill = "lightgreen", alpha = .125)

plot_endpoints <- trials %>%
  filter(calibration == FALSE) %>% 
  ggplot(aes(x = trueInput.x, y = trueInput.z, color = factor(visualXOffset)))+
  target + 
  geom_point() + 
  coord_fixed(ratio = 1)
plot_endpoints
plot_endpoints %>% ggsave(filename = "../figs/plot_endpoints.png", width = 6, height = 6)

plot_tracking <- tracking %>% 
  filter(!(condition == "vispos" & time < trialControlVisibilityOffTime)) %>% 
  mutate(
    type = case_when(
      condition == "truepos" ~ "true pos",
      condition == "vispos" & controllerSphereVisible == TRUE ~ "vis feedback",
      condition == "vispos" & controllerSphereVisible == FALSE ~ "implied pos",
    )
  ) %>% 
  ggplot(aes(x = x, y = z, color = type, group = trialID))+
  target + 
  geom_point(size = 1) + 
  #geom_path(size = 1) +
  coord_fixed(ratio = 1, xlim = c(-0.2, .2)) +
  facet_wrap(~visualXOffset)
  
plot_tracking
plot_tracking %>% ggsave(filename = "../figs/plot_tracking.png",  width = 6, height = 6)
print("File save complete")
